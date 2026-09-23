using System.Text.Json;
using Microsoft.Extensions.Options;
using NSubstitute;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Assistant;
using OrderFlow.Domain.Orders;

namespace OrderFlow.UnitTests.Application;

public class OrderAssistantToolsTests
{
    private readonly IOrderStatistics _statistics = Substitute.For<IOrderStatistics>();
    private readonly OrderAssistantTools _sut;

    public OrderAssistantToolsTests()
    {
        _sut = new OrderAssistantTools(_statistics, Options.Create(new AssistantOptions { TimeZone = "America/Sao_Paulo" }));
    }

    private static JsonElement ToJson(object result) => JsonSerializer.SerializeToElement(result);

    [Fact]
    public async Task CountOrdersAsync_ConverteODiaDeBrasiliaParaUtc()
    {
        await _sut.CountOrdersAsync(de: "2026-09-23", ate: "2026-09-23");

        await _statistics.Received(1).CountAsync(
            Arg.Is<OrderFilter>(f =>
                f.From == new DateTimeOffset(2026, 9, 23, 3, 0, 0, TimeSpan.Zero) &&
                f.To == new DateTimeOffset(2026, 9, 24, 3, 0, 0, TimeSpan.Zero)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CountOrdersAsync_AceitaStatusSemDiferenciarMaiusculas()
    {
        _statistics.CountAsync(Arg.Is<OrderFilter>(f => f.Status == OrderStatus.Pendente), Arg.Any<CancellationToken>()).Returns(3);

        var result = ToJson(await _sut.CountOrdersAsync(status: "pendente"));

        Assert.Equal(3, result.GetProperty("total_pedidos").GetInt32());
    }

    [Fact]
    public async Task SumOrderValuesAsync_RepassaFiltrosDeClienteEProdutoSemEspacos()
    {
        await _sut.SumOrderValuesAsync(cliente: "  Maria ", produto: " Notebook ");

        await _statistics.Received(1).SumValueAsync(
            Arg.Is<OrderFilter>(f => f.Cliente == "Maria" && f.Produto == "Notebook"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SumOrderValuesAsync_FiltrosEmBranco_SaoIgnorados()
    {
        await _sut.SumOrderValuesAsync(cliente: " ", produto: "");

        await _statistics.Received(1).SumValueAsync(
            Arg.Is<OrderFilter>(f => f.Cliente == null && f.Produto == null),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Aprovado")]
    [InlineData("5")]
    public async Task CountOrdersAsync_StatusInvalido_LancaArgumentException(string status)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CountOrdersAsync(status: status));
    }

    [Theory]
    [InlineData("23/09/2026")]
    [InlineData("ontem")]
    public async Task CountOrdersAsync_DataInvalida_LancaArgumentException(string date)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CountOrdersAsync(de: date));
    }

    [Fact]
    public async Task SumOrderValuesAsync_RetornaValorFormatadoEmReais()
    {
        _statistics.SumValueAsync(Arg.Any<OrderFilter>(), Arg.Any<CancellationToken>()).Returns(1234.56m);

        var result = ToJson(await _sut.SumOrderValuesAsync(status: "Finalizado"));

        Assert.Equal(1234.56m, result.GetProperty("valor_total").GetDecimal());
        Assert.Contains("1.234,56", result.GetProperty("valor_formatado").GetString());
    }

    [Fact]
    public async Task AverageProcessingTimeAsync_ConsideraApenasFinalizadosEFormataEmSegundos()
    {
        _statistics.AverageProcessingTimeAsync(Arg.Any<OrderFilter>(), Arg.Any<CancellationToken>())
            .Returns((TimeSpan.FromSeconds(5.1), 4));

        var result = ToJson(await _sut.AverageProcessingTimeAsync());

        await _statistics.Received(1).AverageProcessingTimeAsync(
            Arg.Is<OrderFilter>(f => f.Status == OrderStatus.Finalizado), Arg.Any<CancellationToken>());
        Assert.Equal(4, result.GetProperty("pedidos_finalizados").GetInt32());
        Assert.Equal("5,1 segundos", result.GetProperty("tempo_medio").GetString());
    }

    [Fact]
    public async Task AverageProcessingTimeAsync_SemPedidosFinalizados_InformaAusencia()
    {
        _statistics.AverageProcessingTimeAsync(Arg.Any<OrderFilter>(), Arg.Any<CancellationToken>())
            .Returns(((TimeSpan?)null, 0));

        var result = ToJson(await _sut.AverageProcessingTimeAsync());

        Assert.Equal(0, result.GetProperty("pedidos_finalizados").GetInt32());
    }

    [Fact]
    public async Task GroupOrdersAsync_PorCliente_RetornaGruposFormatados()
    {
        _statistics.GroupAsync(OrderGrouping.Cliente, Arg.Any<OrderFilter>(), 10, Arg.Any<CancellationToken>())
            .Returns([new OrderGroupSummary("Maria Silva", 2, 4689.40m), new OrderGroupSummary("Ana Lima", 1, 1299m)]);

        var result = ToJson(await _sut.GroupOrdersAsync("Cliente"));

        var grupos = result.GetProperty("grupos");
        Assert.Equal("cliente", result.GetProperty("agrupado_por").GetString());
        Assert.Equal(2, grupos.GetArrayLength());
        Assert.Equal("Maria Silva", grupos[0].GetProperty("nome").GetString());
        Assert.Equal(2, grupos[0].GetProperty("total_pedidos").GetInt32());
        Assert.Contains("4.689,40", grupos[0].GetProperty("valor_formatado").GetString());
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 1)]
    [InlineData(500, 50)]
    public async Task GroupOrdersAsync_LimiteFicaEntre1E50(int? limite, int esperado)
    {
        await _sut.GroupOrdersAsync("produto", limite: limite);

        await _statistics.Received(1).GroupAsync(OrderGrouping.Produto, Arg.Any<OrderFilter>(), esperado, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GroupOrdersAsync_AgrupamentoInvalido_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GroupOrdersAsync("cidade"));
    }
}
