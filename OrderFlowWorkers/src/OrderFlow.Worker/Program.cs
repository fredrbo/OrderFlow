using OrderFlow.Worker.Application;
using OrderFlow.Worker.Application.Orders;
using OrderFlow.Worker.Consumers;
using OrderFlow.Worker.Infrastructure;
using OrderFlow.Worker.Messaging;
using OrderFlow.Worker.Resilience;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OrderProcessingOptions>(builder.Configuration.GetSection(OrderProcessingOptions.SectionName));

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddOrderProcessingResilience();

builder.Services.AddHostedService<OrderCreatedConsumer>();

builder.Build().Run();
