using Lothal.BuildingBlocks.Logging;
using Lothal.BuildingBlocks.Telemetry;
using Lothal.Product.Application;
using Lothal.Product.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Lothal.Product.Application.Commands;
using Lothal.Product.Application.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLogging("product-api");
builder.AddCustomTelemetry("product-api");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Layered Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/products/bulk-merge", async (BulkMergeProductsCommand command, [FromServices] IMediator mediator) =>
{
    var result = await mediator.Send(command);
    if (!result) return Results.BadRequest("Invalid product data.");
    
    return Results.Ok();
})
.WithName("BulkMergeProducts")
.WithOpenApi();

app.MapGet("/api/products/{barcode}", async (string barcode, [FromServices] IMediator mediator) =>
{
    var product = await mediator.Send(new GetProductByBarcodeQuery(barcode));
    if (product == null) return Results.NotFound();
    
    return Results.Ok(product);
})
.WithName("GetProductByBarcode")
.WithOpenApi();

app.MapGet("/api/products", async ([FromQuery] int? from, [FromQuery] int? size, [FromServices] IMediator mediator) =>
{
    var products = await mediator.Send(new GetAllProductsQuery(from ?? 0, size ?? 100));
    return Results.Ok(products);
})
.WithName("GetAllProducts")
.WithOpenApi();

app.MapDelete("/api/products/{barcode}", async (string barcode, [FromServices] IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteProductCommand(barcode));
    return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProduct")
.WithOpenApi();

app.Run();
