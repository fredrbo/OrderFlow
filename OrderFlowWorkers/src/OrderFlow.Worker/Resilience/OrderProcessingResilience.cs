using Microsoft.Extensions.Options;
using OrderFlow.Worker.Application.Orders;
using OrderFlow.Worker.Domain.Exceptions;
using Polly;
using Polly.Retry;

namespace OrderFlow.Worker.Resilience;

public static class OrderProcessingResilience
{
    public const string PipelineName = "order-processing";

    public static readonly ResiliencePropertyKey<Guid> OrderIdKey = new("OrderId");

    public static IServiceCollection AddOrderProcessingResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(PipelineName, (pipeline, context) =>
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<OrderProcessingOptions>>().Value;
            var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(PipelineName);

            pipeline.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetryAttempts,
                Delay = options.RetryBaseDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(IsTransient),
                OnRetry = args =>
                {
                    logger.LogWarning(args.Outcome.Exception,
                        "Falha ao processar o pedido {OrderId}. Tentativa {Attempt} de {MaxAttempts} em {Delay:N1}s.",
                        args.Context.Properties.GetValue(OrderIdKey, Guid.Empty),
                        args.AttemptNumber + 1, options.MaxRetryAttempts, args.RetryDelay.TotalSeconds);
                    return ValueTask.CompletedTask;
                }
            });
        });

        return services;
    }

    private static bool IsTransient(Exception exception) =>
        exception is not (OperationCanceledException or DomainException);
}
