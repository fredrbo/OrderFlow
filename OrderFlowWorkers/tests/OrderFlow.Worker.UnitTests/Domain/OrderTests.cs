using OrderFlow.Worker.Domain.Exceptions;
using OrderFlow.Worker.Domain.Orders;

namespace OrderFlow.Worker.UnitTests.Domain;

public class OrderTests
{
    private static Order CreateOrder(OrderStatus status) =>
        new(Guid.NewGuid(), "Maria", "Notebook", 10m, status, DateTimeOffset.UtcNow);

    [Fact]
    public void StartProcessing_QuandoPendente_MudaParaProcessando()
    {
        var order = CreateOrder(OrderStatus.Pendente);
        var startedAt = new DateTimeOffset(2026, 9, 23, 12, 0, 0, TimeSpan.Zero);

        order.StartProcessing(startedAt);

        Assert.Equal(OrderStatus.Processando, order.Status);
        var history = Assert.Single(order.Historico);
        Assert.Equal(order.Id, history.OrderId);
        Assert.Equal(OrderStatus.Processando, history.Status);
        Assert.Equal(startedAt, history.DataAlteracao);
    }

    [Theory]
    [InlineData(OrderStatus.Processando)]
    [InlineData(OrderStatus.Finalizado)]
    public void StartProcessing_QuandoNaoPendente_LancaDomainException(OrderStatus status)
    {
        var order = CreateOrder(status);

        Assert.Throws<DomainException>(() => order.StartProcessing(DateTimeOffset.UtcNow));
        Assert.Empty(order.Historico);
    }

    [Fact]
    public void Finish_QuandoProcessando_MudaParaFinalizadoERegistraData()
    {
        var order = CreateOrder(OrderStatus.Processando);
        var finishedAt = new DateTimeOffset(2026, 9, 23, 12, 0, 5, TimeSpan.Zero);

        order.Finish(finishedAt);

        Assert.Equal(OrderStatus.Finalizado, order.Status);
        Assert.Equal(finishedAt, order.DataFinalizacao);
        var history = Assert.Single(order.Historico);
        Assert.Equal(OrderStatus.Finalizado, history.Status);
        Assert.Equal(finishedAt, history.DataAlteracao);
    }

    [Theory]
    [InlineData(OrderStatus.Pendente)]
    [InlineData(OrderStatus.Finalizado)]
    public void Finish_QuandoNaoProcessando_LancaDomainException(OrderStatus status)
    {
        var order = CreateOrder(status);

        Assert.Throws<DomainException>(() => order.Finish(DateTimeOffset.UtcNow));
        Assert.Empty(order.Historico);
    }
}
