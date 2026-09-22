using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Dtos;

public sealed record OrderResponse(
    Guid Id,
    string Cliente,
    string Produto,
    decimal Valor,
    OrderStatus Status,
    DateTimeOffset DataCriacao)
{
    public static OrderResponse From(Order order) =>
        new(order.Id, order.Cliente, order.Produto, order.Valor, order.Status, order.DataCriacao);
}
