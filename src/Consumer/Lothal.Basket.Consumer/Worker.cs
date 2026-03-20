using Lothal.Basket.Infrastructure.Data;
using Lothal.Basket.Domain.Entities;
using NATS.Client.Core;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Lothal.Basket.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly INatsConnection _natsConnection;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, INatsConnection natsConnection, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _natsConnection = natsConnection;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker starting to listen on NATS subject 'baskets.checkout'");

        await foreach (var msg in _natsConnection.SubscribeAsync<string>("baskets.checkout", cancellationToken: stoppingToken))
        {
            if (msg.Data == null) continue;

            _logger.LogInformation("Received checkout message: {Payload}", msg.Data);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var basket = JsonSerializer.Deserialize<global::Lothal.Basket.Domain.Entities.Basket>(msg.Data, options);

                if (basket == null) continue;

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var trackedCount = dbContext.ChangeTracker.Entries().Count();
                _logger.LogInformation("Processing basket {BasketId}. Tracked entities count: {Count}", basket.Id, trackedCount);

                // Check If Idempotency (Already processed)
                var exists = await dbContext.Baskets.AsNoTracking().AnyAsync(b => b.Id == basket.Id, stoppingToken);
                if (exists)
                {
                    _logger.LogWarning("Basket {BasketId} already processed.", basket.Id);
                    continue;
                }

                try 
                {
                    dbContext.Baskets.Add(basket);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Successfully saved basket {BasketId} to Postgres.", basket.Id);
                }
                catch (InvalidOperationException ioEx) when (ioEx.Message.Contains("already being tracked"))
                {
                    _logger.LogError(ioEx, "Tracking error for basket {BasketId}. Attempting to detach and retry.", basket.Id);
                    // This is a last resort to see what's going on
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing checkout message from NATS");
            }
        }
    }
}
