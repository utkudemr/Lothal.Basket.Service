using Lothal.Product.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Text.Json;

namespace Lothal.Product.Infrastructure.Data;

/// <summary>
/// On startup:
///   1. Seeds the Elasticsearch products index (only if it doesn't already exist).
///   2. Publishes a "stock.upsert" NATS message for every seeded product so that
///      Lothal.Stock picks up matching barcodes and initialises their stock quantities.
///
/// This ensures product and stock data are always correlated — if a product exists
/// in Elasticsearch, a corresponding stock record will exist in the Stock service.
/// </summary>
public class ProductSeederService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductSeederService> _logger;

    public ProductSeederService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ProductSeederService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var seedProducts = new List<Domain.Entities.Product>
        {
            new Domain.Entities.Product { Barcode = "P1001", Name = "T-Shirt",   Class = "Apparel",   Color = "Red",   Size = "M",  Price = 19.99m },
            new Domain.Entities.Product { Barcode = "P1002", Name = "Jeans",     Class = "Apparel",   Color = "Blue",  Size = "32", Price = 49.99m },
            new Domain.Entities.Product { Barcode = "P1003", Name = "Sneakers",  Class = "Footwear",  Color = "White", Size = "42", Price = 89.99m }
        };

        // 1. Seed Elasticsearch (idempotent — only runs if index doesn't exist)
        await repo.SeedDataAsync(seedProducts);

        _logger.LogInformation(
            "Elasticsearch product seed complete — {Count} products ensured", seedProducts.Count);

        // 2. Publish stock.upsert events to NATS for every seeded product.
        //    The Stock service consumes these and initialises stock records in PostgreSQL + Redis.
        //    Each product gets a realistic starting warehouse quantity.
        var stockPayloads = new[]
        {
            new { barcode = "P1001", warehouseQuantity = 100, source = "ProductSeeder" },
            new { barcode = "P1002", warehouseQuantity = 50,  source = "ProductSeeder" },
            new { barcode = "P1003", warehouseQuantity = 30,  source = "ProductSeeder" }
        };

        var natsUrl = _configuration["Nats:Url"] ?? "nats://nats:4222";

        try
        {
            await using var nats = new NatsConnection(new NatsOpts { Url = natsUrl });
            await nats.ConnectAsync();

            foreach (var payload in stockPayloads)
            {
                var json = JsonSerializer.Serialize(payload);
                await nats.PublishAsync("stock.upsert", json, cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Published stock.upsert for Barcode={Barcode} Qty={Qty}",
                    payload.barcode, payload.warehouseQuantity);
            }

            _logger.LogInformation(
                "Stock seed events published via NATS — {Count} products", stockPayloads.Length);
        }
        catch (Exception ex)
        {
            // Non-fatal: stock service may not be up yet, the feed can be replayed
            _logger.LogWarning(ex,
                "Failed to publish stock seed events to NATS. Stock service must be seeded separately.");
        }
    }
}
