namespace OrderFlow.Worker.Application.Orders;

public sealed class OrderProcessingOptions
{
    public const string SectionName = "OrderProcessing";

    public TimeSpan ProcessingDelay { get; init; } = TimeSpan.FromSeconds(5);
    public int MaxRetryAttempts { get; init; } = 3;
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromSeconds(2);
}
