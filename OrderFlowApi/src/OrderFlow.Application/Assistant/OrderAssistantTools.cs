using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Orders;

namespace OrderFlow.Application.Assistant;

public sealed class OrderAssistantTools(IOrderStatistics statistics, IOptions<AssistantOptions> options)
{
    private const int DefaultGroupLimit = 10;
    private const int MaxGroupLimit = 50;

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public async Task<object> CountOrdersAsync(
        [Description("Status do pedido: Pendente, Processando ou Finalizado. Omita para todos.")] string? status = null,
        [Description("Data inicial (inclusiva) no formato yyyy-MM-dd. Omita para sem limite.")] string? de = null,
        [Description("Data final (inclusiva) no formato yyyy-MM-dd. Omita para sem limite.")] string? ate = null,
        [Description("Nome (ou parte do nome) do cliente. Omita para todos.")] string? cliente = null,
        [Description("Nome (ou parte do nome) do produto. Omita para todos.")] string? produto = null,
        CancellationToken cancellationToken = default)
    {
        var total = await statistics.CountAsync(BuildFilter(status, de, ate, cliente, produto), cancellationToken);

        return new { total_pedidos = total };
    }

    public async Task<object> SumOrderValuesAsync(
        [Description("Status do pedido: Pendente, Processando ou Finalizado. Omita para todos.")] string? status = null,
        [Description("Data inicial (inclusiva) no formato yyyy-MM-dd. Omita para sem limite.")] string? de = null,
        [Description("Data final (inclusiva) no formato yyyy-MM-dd. Omita para sem limite.")] string? ate = null,
        [Description("Nome (ou parte do nome) do cliente. Omita para todos.")] string? cliente = null,
        [Description("Nome (ou parte do nome) do produto. Omita para todos.")] string? produto = null,
        CancellationToken cancellationToken = default)
    {
        var total = await statistics.SumValueAsync(BuildFilter(status, de, ate, cliente, produto), cancellationToken);

        return new { valor_total = total, valor_formatado = FormatCurrency(total) };
    }

    public async Task<object> AverageProcessingTimeAsync(
        [Description("Data inicial (inclusiva) de criação no formato yyyy-MM-dd. Omita para sem limite.")] string? de = null,
        [Description("Data final (inclusiva) de criação no formato yyyy-MM-dd. Omita para sem limite.")] string? ate = null,
        [Description("Nome (ou parte do nome) do cliente. Omita para todos.")] string? cliente = null,
        [Description("Nome (ou parte do nome) do produto. Omita para todos.")] string? produto = null,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(nameof(OrderStatus.Finalizado), de, ate, cliente, produto);
        var (average, count) = await statistics.AverageProcessingTimeAsync(filter, cancellationToken);

        return average is null
            ? new { pedidos_finalizados = 0, tempo_medio = "sem pedidos finalizados no período" }
            : new { pedidos_finalizados = count, tempo_medio = FormatDuration(average.Value) };
    }

    public async Task<object> GroupOrdersAsync(
        [Description("Campo de agrupamento: cliente, produto ou status.")] string agrupar_por,
        [Description("Status do pedido: Pendente, Processando ou Finalizado. Omita para todos.")] string? status = null,
        [Description("Data inicial (inclusiva) no formato yyyy-MM-dd. Omita para sem limite.")] string? de = null,
        [Description("Data final (inclusiva) no formato yyyy-MM-dd. Omita para sem limite.")] string? ate = null,
        [Description("Quantidade máxima de grupos, ordenados do maior para o menor valor total. Padrão 10.")] int? limite = null,
        CancellationToken cancellationToken = default)
    {
        var grouping = ParseGrouping(agrupar_por);
        var limit = Math.Clamp(limite ?? DefaultGroupLimit, 1, MaxGroupLimit);

        var groups = await statistics.GroupAsync(grouping, BuildFilter(status, de, ate, null, null), limit, cancellationToken);

        return new
        {
            agrupado_por = grouping.ToString().ToLowerInvariant(),
            grupos = groups.Select(g => new
            {
                nome = g.Nome,
                total_pedidos = g.TotalPedidos,
                valor_total = g.ValorTotal,
                valor_formatado = FormatCurrency(g.ValorTotal)
            })
        };
    }

    private OrderFilter BuildFilter(string? status, string? de, string? ate, string? cliente, string? produto) =>
        new(ParseStatus(status), ToUtcStartOfDay(de), ToUtcStartOfDay(ate)?.AddDays(1), Normalize(cliente), Normalize(produto));

    private DateTimeOffset? ToUtcStartOfDay(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;

        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            throw new ArgumentException($"Data inválida '{date}'. Use o formato yyyy-MM-dd.");

        var localMidnight = parsed.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localMidnight, _timeZone));
    }

    private static OrderStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new ArgumentException($"Status inválido '{status}'. Use Pendente, Processando ou Finalizado.");
    }

    private static OrderGrouping ParseGrouping(string grouping) =>
        Enum.TryParse<OrderGrouping>(grouping, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new ArgumentException($"Agrupamento inválido '{grouping}'. Use cliente, produto ou status.");

    private static string? Normalize(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static string FormatCurrency(decimal value) => value.ToString("C", PtBr);

    private static string FormatDuration(TimeSpan duration) => duration.TotalSeconds < 60
        ? $"{duration.TotalSeconds.ToString("0.#", PtBr)} segundos"
        : duration.TotalMinutes < 60
            ? $"{duration.TotalMinutes.ToString("0.#", PtBr)} minutos"
            : $"{duration.TotalHours.ToString("0.#", PtBr)} horas";
}
