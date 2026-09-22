using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Events;

/// <summary>
/// Contrato da mensagem publicada no RabbitMQ quando um pedido é criado.
/// </summary>
public sealed record OrderCreatedEvent(
    Guid Id,
    string Cliente,
    string Produto,
    decimal Valor,
    OrderStatus Status,
    DateTimeOffset DataCriacao)
{
    public static OrderCreatedEvent From(Order order) =>
        new(order.Id, order.Cliente, order.Produto, order.Valor, order.Status, order.DataCriacao);
}
