using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Infrastructure.Messaging.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(typeof(TimeSpan), "00:00:00.100", "00:05:00")]
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(5);

    [Range(1, 500)]
    public int BatchSize { get; init; } = 20;
}
