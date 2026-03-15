using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Lothal.Mediator.Core;
using Lothal.Mediator.Core.Dispatchers;

namespace Lothal.Basket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHandlers(Assembly.GetExecutingAssembly());
        return services;
    }
}
