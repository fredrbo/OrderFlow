using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Orders.Dtos;

public sealed record OrderDetailsResponse(
    Guid Id,
    string Cliente,
    string Produto,
    decimal Valor,
    OrderStatus Status,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataFinalizacao,
    IReadOnlyList<OrderStatusHistoryResponse> Historico)
{
    public static OrderDetailsResponse From(Order order) =>
        new(
            order.Id,
            order.Cliente,
            order.Produto,
            order.Valor,
            order.Status,
            order.DataCriacao,
            order.DataFinalizacao,
            order.Historico
                .OrderBy(h => h.DataAlteracao)
                .Select(OrderStatusHistoryResponse.From)
                .ToList());
}
