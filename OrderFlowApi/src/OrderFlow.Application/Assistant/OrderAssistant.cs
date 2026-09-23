using System.Globalization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Assistant.Dtos;

namespace OrderFlow.Application.Assistant;

public sealed class OrderAssistant(
    IChatClient chatClient,
    OrderAssistantTools tools,
    TimeProvider timeProvider,
    IOptions<AssistantOptions> options) : IOrderAssistant
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public async Task<AskQuestionResponse> AskAsync(AskQuestionRequest request, CancellationToken cancellationToken)
    {
        List<ChatMessage> messages =
        [
            new(ChatRole.System, BuildSystemPrompt()),
            new(ChatRole.User, request.Pergunta.Trim())
        ];

        var chatOptions = new ChatOptions
        {
            Temperature = 0,
            Tools =
            [
                AIFunctionFactory.Create(tools.CountOrdersAsync, "contar_pedidos",
                    "Conta pedidos, com filtros opcionais de status, período de criação, cliente e produto."),
                AIFunctionFactory.Create(tools.SumOrderValuesAsync, "somar_valor_pedidos",
                    "Soma o valor dos pedidos em reais, com filtros opcionais de status, período de criação, cliente e produto."),
                AIFunctionFactory.Create(tools.AverageProcessingTimeAsync, "tempo_medio_processamento",
                    "Calcula o tempo médio entre a criação e a finalização (aprovação) dos pedidos finalizados."),
                AIFunctionFactory.Create(tools.GroupOrdersAsync, "agrupar_pedidos",
                    "Agrupa pedidos por cliente, produto ou status, retornando quantidade e valor total de cada grupo, do maior para o menor valor.")
            ]
        };

        try
        {
            var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
            return new AskQuestionResponse(response.Text.Trim());
        }
        catch (HttpRequestException ex)
        {
            throw new AssistantUnavailableException(ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AssistantUnavailableException(ex);
        }
    }

    private string BuildSystemPrompt()
    {
        var now = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _timeZone);

        return $"""
            Você é o assistente do OrderFlow, um sistema de gestão de pedidos.
            Responda sempre em português do Brasil, de forma amigável, curta e objetiva.

            Regras:
            - Use as ferramentas para obter qualquer número. Nunca invente ou estime valores.
            - Use exatamente os números e textos formatados retornados pelas ferramentas.
            - Se a pergunta não for sobre os pedidos do OrderFlow, diga educadamente que só pode ajudar com pedidos.
            - Status possíveis: Pendente, Processando e Finalizado. "Aprovado" significa Finalizado.
            - Para perguntas sobre um cliente ou produto específico, use o filtro "cliente" ou "produto" com o nome citado.
            - Para perguntas "por cliente", "por produto", "por status" ou de ranking (quem mais comprou, produto mais vendido), use agrupar_pedidos.
            - Ao listar grupos, apresente um item por linha no formato "Nome: valor (N pedidos)".

            Contexto de data:
            - Agora é {now.ToString("dddd, dd/MM/yyyy HH:mm", PtBr)} (horário de Brasília). Hoje é {now:yyyy-MM-dd}.
            - Passe datas para as ferramentas no formato yyyy-MM-dd; "de" e "ate" são inclusivos.
            - "Hoje" é de={now:yyyy-MM-dd} e ate={now:yyyy-MM-dd}. "Este mês" começa em {now:yyyy-MM}-01 e vai até hoje.
            """;
    }
}
