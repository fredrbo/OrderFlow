using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OrderFlow.Worker.Application.Abstractions;
using OrderFlow.Worker.Application.Orders;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.UnitTests.Application;

public class OrderProcessorTests
{
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(5);

    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly FakeTimeProvider _time = new();
    private readonly OrderProcessor _sut;

    public OrderProcessorTests()
    {
        _sut = new OrderProcessor(
            _repository,
            _time,
            Options.Create(new OrderProcessingOptions { ProcessingDelay = Delay }),
            NullLogger<OrderProcessor>.Instance);
    }

    private Order GivenOrder(OrderStatus status)
    {
        var order = new Order(Guid.NewGuid(), "Maria", "Notebook", 10m, status, DateTimeOffset.UtcNow);
        _repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);
        return order;
    }

    [Fact]
    public async Task ProcessAsync_PedidoPendente_FicaProcessandoEFinalizaAposODelay()
    {
        var order = GivenOrder(OrderStatus.Pendente);

        var processing = _sut.ProcessAsync(order.Id, CancellationToken.None);

        Assert.Equal(OrderStatus.Processando, order.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.False(processing.IsCompleted);

        _time.Advance(Delay - TimeSpan.FromMilliseconds(1));
        Assert.False(processing.IsCompleted);

        _time.Advance(TimeSpan.FromMilliseconds(1));
        await processing;

        Assert.Equal(OrderStatus.Finalizado, order.Status);
        await _repository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_PedidoJaProcessando_RetomaEFinaliza()
    {
        var order = GivenOrder(OrderStatus.Processando);

        var processing = _sut.ProcessAsync(order.Id, CancellationToken.None);
        _time.Advance(Delay);
        await processing;

        Assert.Equal(OrderStatus.Finalizado, order.Status);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_PedidoJaFinalizado_NaoAlteraNada()
    {
        var order = GivenOrder(OrderStatus.Finalizado);

        await _sut.ProcessAsync(order.Id, CancellationToken.None);

        Assert.Equal(OrderStatus.Finalizado, order.Status);
        await _repository.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task ProcessAsync_PedidoInexistente_NaoAlteraNada()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        await _sut.ProcessAsync(Guid.NewGuid(), CancellationToken.None);

        await _repository.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task ProcessAsync_ConflitoAoIniciar_IgnoraSemFinalizar()
    {
        var order = GivenOrder(OrderStatus.Pendente);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ConcurrencyConflictException(new Exception())));

        await _sut.ProcessAsync(order.Id, CancellationToken.None);

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(OrderStatus.Finalizado, order.Status);
    }

    [Fact]
    public async Task ProcessAsync_ConflitoAoFinalizar_IgnoraSemErro()
    {
        var order = GivenOrder(OrderStatus.Processando);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ConcurrencyConflictException(new Exception())));

        var processing = _sut.ProcessAsync(order.Id, CancellationToken.None);
        _time.Advance(Delay);
        await processing;

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_CanceladoDuranteODelay_PermaneceProcessando()
    {
        var order = GivenOrder(OrderStatus.Pendente);
        using var cts = new CancellationTokenSource();

        var processing = _sut.ProcessAsync(order.Id, cts.Token);
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => processing);
        Assert.Equal(OrderStatus.Processando, order.Status);
    }
}
