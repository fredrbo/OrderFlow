using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Messaging.Outbox;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    RabbitMqPublisher publisher,
    OutboxSignal signal,
    TimeProvider timeProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var hasMore = false;

            try
            {
                hasMore = await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao processar a outbox.");
            }

            if (!hasMore)
                await signal.WaitAsync(_options.PollingInterval, stoppingToken);
        }
    }

    private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var messages = await dbContext.OutboxMessages
                .FromSql($"""
                    SELECT * FROM outbox_messages
                    WHERE processed_at IS NULL
                    ORDER BY occurred_at
                    LIMIT {_options.BatchSize}
                    FOR UPDATE SKIP LOCKED
                    """)
                .ToListAsync(cancellationToken);

            var published = 0;

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message, cancellationToken);
                    message.MarkAsProcessed(timeProvider.GetUtcNow());
                    published++;

                    logger.LogInformation("Mensagem {MessageId} ({Type}) publicada a partir da outbox.",
                        message.Id, message.Type);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    message.MarkAsFailed(ex.Message);

                    logger.LogWarning(ex, "Falha ao publicar a mensagem {MessageId} ({Type}). Tentativa {Attempts}.",
                        message.Id, message.Type, message.Attempts);
                    break;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return published == _options.BatchSize;
        });
    }
}
