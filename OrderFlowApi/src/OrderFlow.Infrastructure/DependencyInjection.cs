using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Assistant;
using OrderFlow.Infrastructure.Ai;
using OrderFlow.Infrastructure.Messaging;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Queries;
using OrderFlow.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderFlow")
            ?? throw new InvalidOperationException("Connection string 'OrderFlow' não configurada.");

        services.AddDbContext<OrderFlowDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatistics, OrderStatistics>();

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IOrderEventPublisher, RabbitMqOrderEventPublisher>();

        services.AddLlm(configuration);

        return services;
    }

    private static void AddLlm(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AssistantOptions>(configuration.GetSection(AssistantOptions.SectionName));

        var llmSection = configuration.GetSection(LlmOptions.SectionName);
        var maxToolIterations = llmSection.Get<LlmOptions>()?.MaxToolIterations ?? new LlmOptions().MaxToolIterations;

        services.AddOptions<LlmOptions>()
            .Bind(llmSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddChatClient(sp =>
            {
                var options = sp.GetRequiredService<IOptions<LlmOptions>>().Value;
                var httpClient = new HttpClient { BaseAddress = new Uri(options.Endpoint), Timeout = options.Timeout };
                return new OllamaApiClient(httpClient, options.Model);
            })
            .UseFunctionInvocation(configure: client => client.MaximumIterationsPerRequest = maxToolIterations);

        services.AddHostedService<LlmWarmupService>();
    }
}
