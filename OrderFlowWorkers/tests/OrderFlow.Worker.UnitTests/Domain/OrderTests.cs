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

        order.StartProcessing();

        Assert.Equal(OrderStatus.Processando, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Processando)]
    [InlineData(OrderStatus.Finalizado)]
    public void StartProcessing_QuandoNaoPendente_LancaDomainException(OrderStatus status)
    {
        var order = CreateOrder(status);

        Assert.Throws<DomainException>(order.StartProcessing);
    }

    [Fact]
    public void Finish_QuandoProcessando_MudaParaFinalizado()
    {
        var order = CreateOrder(OrderStatus.Processando);

        order.Finish();

        Assert.Equal(OrderStatus.Finalizado, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pendente)]
    [InlineData(OrderStatus.Finalizado)]
    public void Finish_QuandoNaoProcessando_LancaDomainException(OrderStatus status)
    {
        var order = CreateOrder(status);

        Assert.Throws<DomainException>(order.Finish);
    }
}
