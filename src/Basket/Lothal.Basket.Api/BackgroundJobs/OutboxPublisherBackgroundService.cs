using Lothal.Basket.Infrastructure.Data;
using NATS.Client.Core;
using Microsoft.EntityFrameworkCore;

namespace Lothal.Basket.Api.BackgroundJobs;

public class OutboxPublisherBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INatsConnection _natsConnection;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;

    public OutboxPublisherBackgroundService(
        IServiceProvider serviceProvider,
        INatsConnection natsConnection,
        ILogger<OutboxPublisherBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _natsConnection = natsConnection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            // Wait 5 seconds before polling again
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                // Publish to NATS
                // Subject is baskets.events
                await _natsConnection.PublishAsync("baskets.events", message.Payload, cancellationToken: cancellationToken);

                message.ProcessedOn = DateTime.UtcNow;
                _logger.LogInformation("Published outbox message {MessageId} to NATS.", message.Id);
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}.", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
