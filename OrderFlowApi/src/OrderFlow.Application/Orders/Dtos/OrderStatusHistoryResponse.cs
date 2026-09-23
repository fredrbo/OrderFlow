using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Dtos;

public sealed record OrderStatusHistoryResponse(OrderStatus Status, DateTimeOffset DataAlteracao)
{
    public static OrderStatusHistoryResponse From(OrderStatusHistory history) =>
        new(history.Status, history.DataAlteracao);
}
