using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lothal.Stock.Application;

public static class StockApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddStockApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StockApplicationServiceCollectionExtensions).Assembly));
        return services;
    }
}
