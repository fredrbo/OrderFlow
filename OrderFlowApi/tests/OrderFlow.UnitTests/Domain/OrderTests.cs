using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTests.Domain;

public class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ComDadosValidos_CriaPedidoPendente()
    {
        var order = Order.Create("  Maria  ", " Notebook ", 4599.90m, Now);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal("Maria", order.Cliente);
        Assert.Equal("Notebook", order.Produto);
        Assert.Equal(4599.90m, order.Valor);
        Assert.Equal(OrderStatus.Pendente, order.Status);
        Assert.Equal(Now, order.DataCriacao);
    }

    [Theory]
    [InlineData("", "Notebook", 10)]
    [InlineData("   ", "Notebook", 10)]
    [InlineData("Maria", "", 10)]
    [InlineData("Maria", "Notebook", 0)]
    [InlineData("Maria", "Notebook", -1)]
    public void Create_ComDadosInvalidos_LancaDomainException(string cliente, string produto, decimal valor)
    {
        Assert.Throws<DomainException>(() => Order.Create(cliente, produto, valor, Now));
    }

    [Fact]
    public void Create_ComClienteMaiorQueOLimite_LancaDomainException()
    {
        var cliente = new string('a', Order.ClienteMaxLength + 1);

        Assert.Throws<DomainException>(() => Order.Create(cliente, "Notebook", 10m, Now));
    }
}
