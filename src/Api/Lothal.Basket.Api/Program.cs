using Lothal.Basket.Application;
using Lothal.Basket.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using Lothal.Basket.Application.Queries;
using Lothal.Basket.Infrastructure;
using Lothal.Basket.Infrastructure.Data;
using Lothal.Mediator.Core.Dispatchers;
using NATS.Client.Core;
using NATS.Net;
using Lothal.Basket.Api;
using Lothal.BuildingBlocks.Logging;
using Lothal.BuildingBlocks.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLogging("basket-api");
builder.AddCustomTelemetry("basket-api");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Layered Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add NATS Client
var natsUrl = builder.Configuration.GetValue<string>("Nats:Url") ?? "nats://127.0.0.1:4222";
builder.Services.AddSingleton<INatsConnection>(sp => new NatsConnection(new NatsOpts { Url = natsUrl }));

// Add Background Service
builder.Services.AddHostedService<Lothal.Basket.Api.BackgroundJobs.OutboxPublisherBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Apply DB migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.MapPost("/api/baskets", async (CreateBasketCommand command, [FromServices] Mediator mediator) =>
{
    var basketId = await mediator.Send(command);
    return Results.Created($"/api/baskets/{basketId}", new { Id = basketId, ServedBy = Environment.MachineName });
})
.WithName("CreateBasket")
.WithOpenApi();

app.MapGet("/api/baskets/{id}", async (Guid id, [FromServices] Mediator mediator) =>
{
    var basket = await mediator.Send(new GetBasketByIdQuery(id));
    if (basket == null) return Results.NotFound();
    
    return Results.Ok(new { Basket = basket, ServedBy = Environment.MachineName });
})
.WithName("GetBasketById")
.WithOpenApi();

app.Run();
