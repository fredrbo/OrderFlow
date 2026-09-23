using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Worker.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required] public string HostName { get; init; } = "localhost";
    [Range(1, 65535)] public int Port { get; init; } = 5672;
    [Required] public string UserName { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
    [Required] public string VirtualHost { get; init; } = "/";

    [Required] public string Exchange { get; init; } = "orderflow.orders";
    [Required] public string OrderCreatedQueue { get; init; } = "orderflow.order-created";
    [Required] public string OrderCreatedRoutingKey { get; init; } = "order.created";
    [Required] public string OrderCreatedDeadLetterQueue { get; init; } = "orderflow.order-created.dlq";

    [Range(1, 100)] public ushort MaxConcurrentMessages { get; init; } = 5;
    public TimeSpan ConnectionRetryDelay { get; init; } = TimeSpan.FromSeconds(5);
}
