using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lothal.Stock.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStockApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
