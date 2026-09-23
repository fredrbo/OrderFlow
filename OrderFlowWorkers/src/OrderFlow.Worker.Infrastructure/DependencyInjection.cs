using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Worker.Application.Abstractions;
using OrderFlow.Worker.Infrastructure.Persistence;
using OrderFlow.Worker.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Worker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderFlow")
            ?? throw new InvalidOperationException("Connection string 'OrderFlow' não configurada.");

        services.AddDbContext<OrderFlowDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
