using Lothal.Basket.Consumer;
using NATS.Client.Core;
using Lothal.BuildingBlocks.Logging;
using Lothal.BuildingBlocks.Telemetry;
using Lothal.Basket.Infrastructure.Data;
using Lothal.BuildingBlocks.Messaging;
using Lothal.Basket.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.AddCustomLogging("basket-consumer");
builder.AddCustomTelemetry("basket-consumer");

// Configure NATS
builder.AddCustomNats();

// Configure Infrastructure (Postgres, Redis, Resilience)
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
