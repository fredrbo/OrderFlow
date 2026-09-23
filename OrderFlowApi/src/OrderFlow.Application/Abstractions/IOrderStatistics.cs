using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Abstractions;

public sealed record OrderFilter(
    OrderStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Cliente = null,
    string? Produto = null);

public enum OrderGrouping
{
    Cliente,
    Produto,
    Status
}

public sealed record OrderGroupSummary(string Nome, int TotalPedidos, decimal ValorTotal);

public interface IOrderStatistics
{
    Task<int> CountAsync(OrderFilter filter, CancellationToken cancellationToken);
    Task<decimal> SumValueAsync(OrderFilter filter, CancellationToken cancellationToken);
    Task<(TimeSpan? Average, int Count)> AverageProcessingTimeAsync(OrderFilter filter, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderGroupSummary>> GroupAsync(OrderGrouping grouping, OrderFilter filter, int limit, CancellationToken cancellationToken);
}
