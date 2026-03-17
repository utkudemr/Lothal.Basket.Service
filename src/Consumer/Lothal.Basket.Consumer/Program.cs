using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Lothal.Basket.Consumer;
using NATS.Client.Core;
using Lothal.BuildingBlocks.Logging;
using Lothal.BuildingBlocks.Telemetry;

var builder = Host.CreateApplicationBuilder(args);

builder.AddCustomLogging("basket-consumer");
builder.AddCustomTelemetry("basket-consumer");

// Configure NATS
var natsUrl = builder.Configuration.GetValue<string>("Nats:Url") ?? "nats://127.0.0.1:4222";
builder.Services.AddSingleton<INatsConnection>(sp => new NatsConnection(new NatsOpts { Url = natsUrl }));

// Configure Couchbase
builder.Services.AddCouchbase(options =>
{
    options.ConnectionString = builder.Configuration.GetValue<string>("Couchbase:ConnectionString") ?? "couchbase://127.0.0.1";
    options.UserName = builder.Configuration.GetValue<string>("Couchbase:Username") ?? "Administrator";
    options.Password = builder.Configuration.GetValue<string>("Couchbase:Password") ?? "password";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
