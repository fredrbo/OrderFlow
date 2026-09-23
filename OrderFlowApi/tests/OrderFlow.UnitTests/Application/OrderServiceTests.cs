using NSubstitute;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Dtos;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTests.Application;

public class OrderServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);

    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly IOrderEventOutbox _outbox = Substitute.For<IOrderEventOutbox>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_repository, _outbox, _unitOfWork, new FixedTimeProvider(Now));
    }

    [Fact]
    public async Task CreateAsync_RegistraPedidoEEventoNaMesmaTransacao()
    {
        var request = new CreateOrderRequest("Maria", "Notebook", 4599.90m);

        var response = await _sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal(OrderStatus.Pendente, response.Status);
        Assert.Equal(Now, response.DataCriacao);

        Received.InOrder(() =>
        {
            _repository.Add(Arg.Is<Order>(o => o.Id == response.Id));
            _outbox.Add(Arg.Is<OrderCreatedEvent>(e => e.Id == response.Id));
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ComDadosInvalidos_NaoRegistraNemSalva()
    {
        var request = new CreateOrderRequest("", "Notebook", 0);

        await Assert.ThrowsAsync<DomainException>(() => _sut.CreateAsync(request, CancellationToken.None));

        _repository.DidNotReceiveWithAnyArgs().Add(default!);
        _outbox.DidNotReceiveWithAnyArgs().Add(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task CreateAsync_QuandoPersistenciaFalha_PropagaErro()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("db down")));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(new CreateOrderRequest("Maria", "Notebook", 10m), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_QuandoNaoExiste_RetornaNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var response = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetByIdAsync_QuandoExiste_RetornaDetalhesComHistorico()
    {
        var order = Order.Create("Maria", "Notebook", 10m, Now);
        _repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var response = await _sut.GetByIdAsync(order.Id, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(order.Id, response.Id);
        var history = Assert.Single(response.Historico);
        Assert.Equal(OrderStatus.Pendente, history.Status);
        Assert.Equal(Now, history.DataAlteracao);
    }

    [Fact]
    public async Task ListAsync_RetornaPedidosMapeados()
    {
        var order = Order.Create("Maria", "Notebook", 10m, Now);
        _repository.ListAsync(Arg.Any<CancellationToken>()).Returns([order]);

        var response = await _sut.ListAsync(CancellationToken.None);

        var item = Assert.Single(response);
        Assert.Equal(order.Id, item.Id);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
