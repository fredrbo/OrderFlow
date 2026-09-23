namespace OrderFlow.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessage
{
    public const int LastErrorMaxLength = 2000;

    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload, DateTimeOffset occurredAt) =>
        new()
        {
            Id = Guid.CreateVersion7(occurredAt),
            Type = type,
            Payload = payload,
            OccurredAt = occurredAt
        };

    public void MarkAsProcessed(DateTimeOffset processedAt)
    {
        Attempts++;
        ProcessedAt = processedAt;
        LastError = null;
    }

    public void MarkAsFailed(string error)
    {
        Attempts++;
        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;
    }
}
