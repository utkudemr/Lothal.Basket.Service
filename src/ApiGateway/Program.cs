using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

// Add the reverse proxy to capability to the server
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Register the custom HTTP client factory
builder.Services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
