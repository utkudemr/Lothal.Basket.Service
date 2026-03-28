using Lothal.Stock.Api;
using Lothal.BuildingBlocks.Logging;
using Lothal.BuildingBlocks.Telemetry;
using Lothal.Stock.Application.Commands;
using Lothal.Stock.Application.Interfaces;
using Lothal.Stock.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Lothal.Stock.Application;
using Lothal.Stock.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Observability ─────────────────────────────────────────────────────────────
builder.AddCustomLogging("stock-api");
builder.AddCustomTelemetry("stock-api");

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Application + Infrastructure ─────────────────────────────────────────────
builder.Services.AddStockApplicationServices();
builder.Services.AddStockInfrastructureServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// ── GET /api/stocks/{barcode} ─────────────────────────────────────────────────
app.MapGet("/api/stocks/{barcode}", async (string barcode, [FromServices] IMediator mediator) =>
{
    var stock = await mediator.Send(new GetStockByBarcodeQuery(barcode));
    return stock is null ? Results.NotFound() : Results.Ok(stock);
})
.WithName("GetStockByBarcode")
.WithOpenApi();

// ── PUT /api/stocks/upsert  (manual feed / test endpoint) ────────────────────
app.MapPut("/api/stocks/upsert", async (UpsertStockCommand command, [FromServices] IMediator mediator) =>
{
    await mediator.Send(command);
    return Results.Ok();
})
.WithName("UpsertStock")
.WithOpenApi();

// ── POST /api/stocks/{barcode}/reserve ───────────────────────────────────────
app.MapPost("/api/stocks/{barcode}/reserve", async (
    string barcode,
    [FromBody] StockQuantityRequest request,
    [FromServices] IMediator mediator) =>
{
    var result = await mediator.Send(new ReserveStockCommand(barcode, request.Quantity));

    return result.Status switch
    {
        ReservationStatus.Success => Results.Ok(new { reserved = request.Quantity, remaining = result.AvailableQuantity }),
        ReservationStatus.InsufficientStock => Results.Conflict(new { reason = "InsufficientStock", available = result.AvailableQuantity }),
        ReservationStatus.NotFound => Results.NotFound(new { reason = "StockNotFound", barcode }),
        _ => Results.Problem("Unexpected reservation error")
    };
})
.WithName("ReserveStock")
.WithOpenApi();

app.MapPost("/api/stocks/release", async (
    [FromBody] ReleaseStockCommand command,
    [FromServices] IMediator mediator) =>
{
    await mediator.Send(command);
    return Results.Ok();
})
.WithName("ReleaseStockGlobal")
.WithOpenApi();

app.MapPost("/api/stocks/bulk-increase", async (
    [FromBody] BulkIncreaseStockCommand command,
    [FromServices] IMediator mediator) =>
{
    await mediator.Send(command);
    return Results.Ok();
})
.WithName("BulkIncreaseStock")
.WithOpenApi();

app.Run();
