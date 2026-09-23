using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Infrastructure.Persistence.Queries;

internal sealed class OrderStatistics(OrderFlowDbContext dbContext) : IOrderStatistics
{
    public Task<int> CountAsync(OrderFilter filter, CancellationToken cancellationToken) =>
        Apply(filter).CountAsync(cancellationToken);

    public async Task<decimal> SumValueAsync(OrderFilter filter, CancellationToken cancellationToken) =>
        await Apply(filter).SumAsync(o => (decimal?)o.Valor, cancellationToken) ?? 0m;

    public async Task<(TimeSpan? Average, int Count)> AverageProcessingTimeAsync(OrderFilter filter, CancellationToken cancellationToken)
    {
        var result = await Apply(filter)
            .Where(o => o.DataFinalizacao != null)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Average = EF.Functions.Average(g.Select(o => o.DataFinalizacao!.Value - o.DataCriacao)),
                Count = g.Count()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return result is null ? (null, 0) : (result.Average, result.Count);
    }

    public async Task<IReadOnlyList<OrderGroupSummary>> GroupAsync(
        OrderGrouping grouping, OrderFilter filter, int limit, CancellationToken cancellationToken)
    {
        var query = Apply(filter);

        var groups = grouping switch
        {
            OrderGrouping.Cliente => query.GroupBy(o => o.Cliente)
                .Select(g => new { Nome = g.Key, TotalPedidos = g.Count(), ValorTotal = g.Sum(o => o.Valor) }),
            OrderGrouping.Produto => query.GroupBy(o => o.Produto)
                .Select(g => new { Nome = g.Key, TotalPedidos = g.Count(), ValorTotal = g.Sum(o => o.Valor) }),
            OrderGrouping.Status => query.GroupBy(o => o.Status)
                .Select(g => new { Nome = g.Key.ToString(), TotalPedidos = g.Count(), ValorTotal = g.Sum(o => o.Valor) }),
            _ => throw new ArgumentOutOfRangeException(nameof(grouping), grouping, null)
        };

        var rows = await groups
            .OrderByDescending(g => g.ValorTotal)
            .ThenBy(g => g.Nome)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rows.Select(r => new OrderGroupSummary(r.Nome, r.TotalPedidos, r.ValorTotal)).ToList();
    }

    private IQueryable<Order> Apply(OrderFilter filter)
    {
        var query = dbContext.Orders.AsNoTracking();

        if (filter.Status is not null)
            query = query.Where(o => o.Status == filter.Status);

        if (filter.From is not null)
            query = query.Where(o => o.DataCriacao >= filter.From);

        if (filter.To is not null)
            query = query.Where(o => o.DataCriacao < filter.To);

        if (filter.Cliente is not null)
            query = query.Where(o => EF.Functions.ILike(o.Cliente, ContainsPattern(filter.Cliente)));

        if (filter.Produto is not null)
            query = query.Where(o => EF.Functions.ILike(o.Produto, ContainsPattern(filter.Produto)));

        return query;
    }

    private static string ContainsPattern(string text) =>
        $"%{text.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
}
