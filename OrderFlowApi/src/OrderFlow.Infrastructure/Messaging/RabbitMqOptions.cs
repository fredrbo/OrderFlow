using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required] public string HostName { get; init; } = "localhost";
    [Range(1, 65535)] public int Port { get; init; } = 5672;
    // Credenciais sem valor padrão: devem vir da configuração (appsettings/variáveis de ambiente)
    [Required] public string UserName { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
    [Required] public string VirtualHost { get; init; } = "/";

    [Required] public string Exchange { get; init; } = "orderflow.orders";
    [Required] public string OrderCreatedQueue { get; init; } = "orderflow.order-created";
    [Required] public string OrderCreatedRoutingKey { get; init; } = "order.created";
}
