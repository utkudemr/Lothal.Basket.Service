using Lothal.Stock.Application.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using System.Text.Json;

namespace Lothal.Stock.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes stock upsert events from NATS subject "stock.upsert".
/// Simulates what an ERP/WMS integration would push in production (thousands of records).
///
/// Observability:
///   - Structured logs for each consumed message (barcode, qty, source)
///   - Warning logs for malformed payloads (no ES call — fast, decoupled)
/// </summary>
public class StockNatsConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StockNatsConsumer> _logger;

    public StockNatsConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<StockNatsConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var natsUrl = _configuration["Nats:Url"] ?? "nats://nats:4222";
        var opts = new NatsOpts { Url = natsUrl };

        _logger.LogInformation("StockNatsConsumer connecting to NATS at {Url}", natsUrl);

        await using var connection = new NatsConnection(opts);
        await connection.ConnectAsync();

        _logger.LogInformation("StockNatsConsumer subscribed to subject 'stock.upsert'");

        await foreach (var msg in connection.SubscribeAsync<string>("stock.upsert", cancellationToken: stoppingToken))
        {
            if (msg.Data is null) continue;

            StockUpsertMessage? payload = null;
            try
            {
                payload = JsonSerializer.Deserialize<StockUpsertMessage>(msg.Data,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "StockNatsConsumer — malformed JSON payload, skipping");
                continue;
            }

            if (payload is null || string.IsNullOrWhiteSpace(payload.Barcode))
            {
                _logger.LogWarning("StockNatsConsumer — missing barcode in payload, skipping");
                continue;
            }

            _logger.LogInformation(
                "StockNatsConsumer received — Barcode={Barcode} Qty={Qty} Source={Source}",
                payload.Barcode, payload.WarehouseQuantity, payload.Source);

            // Each message gets its own DI scope (scoped services, proper disposal)
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(
                new UpsertStockCommand(payload.Barcode, payload.WarehouseQuantity, payload.Source ?? "NATS"),
                stoppingToken);
        }
    }
}

/// <summary>Wire-format for stock.upsert NATS messages.</summary>
public record StockUpsertMessage(string Barcode, int WarehouseQuantity, string? Source);
