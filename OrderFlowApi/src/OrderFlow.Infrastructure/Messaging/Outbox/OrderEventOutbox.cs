using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Orders.Events;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Messaging.Outbox;

internal sealed class OrderEventOutbox(
    OrderFlowDbContext dbContext,
    IOptions<RabbitMqOptions> options,
    TimeProvider timeProvider) : IOrderEventOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };

    public void Add(OrderCreatedEvent @event) =>
        dbContext.OutboxMessages.Add(OutboxMessage.Create(
            options.Value.OrderCreatedRoutingKey,
            JsonSerializer.Serialize(@event, SerializerOptions),
            timeProvider.GetUtcNow()));
}
