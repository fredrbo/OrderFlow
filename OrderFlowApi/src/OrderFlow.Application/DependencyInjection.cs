using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderFlow.Application.Assistant;
using OrderFlow.Application.Orders;

namespace OrderFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<OrderAssistantTools>();
        services.AddScoped<IOrderAssistant, OrderAssistant>();

        return services;
    }
}
