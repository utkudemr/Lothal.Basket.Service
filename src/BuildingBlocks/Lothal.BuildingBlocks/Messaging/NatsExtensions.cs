using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using NATS.Client.Core;
using NATS.Net;

namespace Lothal.BuildingBlocks.Messaging;

public static class NatsExtensions
{
    public static IHostApplicationBuilder AddCustomNats(this IHostApplicationBuilder builder)
    {
        var natsUrl = builder.Configuration.GetValue<string>("Nats:Url") ?? "nats://127.0.0.1:4222";
        
        builder.Services.AddSingleton<INatsConnection>(sp => new NatsConnection(new NatsOpts { Url = natsUrl }));
        
        return builder;
    }
}
