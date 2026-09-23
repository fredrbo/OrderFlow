using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Infrastructure.Ai;

internal sealed class LlmWarmupService(IChatClient chatClient, ILogger<LlmWarmupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await chatClient.GetResponseAsync("ok", new ChatOptions { MaxOutputTokens = 1 }, stoppingToken);
            logger.LogInformation("Modelo de linguagem carregado e pronto para responder.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning("Não foi possível pré-carregar o modelo de linguagem: {Message}", ex.Message);
        }
    }
}
