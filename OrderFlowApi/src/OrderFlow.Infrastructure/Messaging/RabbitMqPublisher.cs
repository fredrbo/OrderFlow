using System.Text;
using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Messaging.Outbox;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure.Messaging;

internal sealed class RabbitMqPublisher(RabbitMqConnection connection, IOptions<RabbitMqOptions> options)
{
    private static readonly CreateChannelOptions ChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var conn = await connection.GetAsync(cancellationToken);
        await using var channel = await conn.CreateChannelAsync(ChannelOptions, cancellationToken);

        var properties = new BasicProperties
        {
            MessageId = message.Id.ToString(),
            ContentType = "application/json",
            Type = message.Type,
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(message.OccurredAt.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            _options.Exchange, message.Type, mandatory: false,
            properties, Encoding.UTF8.GetBytes(message.Payload), cancellationToken);
    }
}
