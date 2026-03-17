using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Forwarder;
using Lothal.BuildingBlocks.Logging;
using Lothal.BuildingBlocks.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLogging("api-gateway");
builder.AddCustomTelemetry("api-gateway");

// Add the reverse proxy to capability to the server
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Register the custom HTTP client factory
builder.Services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();

builder.Services.AddRateLimiter(options =>
{
    // Policy for GET /api/baskets/{id} (Allow more requests)
    options.AddFixedWindowLimiter("get-basket-policy", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    // Policy for POST /api/baskets (Allow fewer requests)
    options.AddFixedWindowLimiter("create-basket-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRateLimiter();
app.MapReverseProxy();

app.Run();

// Custom HTTP Client Factory to disable long-lived connection pools
// This forces HttpClient to resolve DNS again natively handling Docker DNS Load Balancing!
public class CustomForwarderHttpClientFactory : ForwarderHttpClientFactory
{
    protected override void ConfigureHandler(ForwarderHttpClientContext context, SocketsHttpHandler handler)
    {
        base.ConfigureHandler(context, handler);
        // This is the magic touch! Force a connection to close after 1 second of being pooled.
        // As a result, new requests create new connections, bouncing back and forth through Docker's Round-Robin DNS.
        handler.PooledConnectionLifetime = TimeSpan.FromSeconds(1);
    }
}
