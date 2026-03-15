using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Lothal.Basket.Consumer.Models;
using NATS.Client.Core;
using System.Text.Json;

namespace Lothal.Basket.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly INatsConnection _natsConnection;
    private readonly IBucketProvider _bucketProvider;

    public Worker(ILogger<Worker> logger, INatsConnection natsConnection, IBucketProvider bucketProvider)
    {
        _logger = logger;
        _natsConnection = natsConnection;
        _bucketProvider = bucketProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Couchbase'in tamamen bootstrap olmasını bekle (retry logic)
        ICouchbaseCollection? collection = null;
        while (!stoppingToken.IsCancellationRequested && collection == null)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync("basket");
                collection = bucket.DefaultCollection();
                _logger.LogInformation("Connected to Couchbase bucket 'basket'.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Couchbase not ready yet, retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        if (collection == null) return;

        _logger.LogInformation("Worker starting to listen on NATS subject 'baskets.events'");

        await foreach (var msg in _natsConnection.SubscribeAsync<string>("baskets.events", cancellationToken: stoppingToken))
        {
            if (msg.Data == null) continue;

            _logger.LogInformation("Received message payload: {Payload}", msg.Data);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var basketDto = JsonSerializer.Deserialize<BasketDocument>(msg.Data, options);

                using var jsonDoc = JsonDocument.Parse(msg.Data);
                var root = jsonDoc.RootElement;
                if (!root.TryGetProperty("Id", out var idProp) || !idProp.TryGetGuid(out var basketId))
                    continue;

                if (basketDto == null) continue;
                basketDto.BasketId = basketId;
                basketDto.Id = $"basket::{basketId}";

                // Simple Idempotency Check using Couchbase Document (Inbox Pattern)
                var inboxId = $"inbox::{basketDto.BasketId}";
                try
                {
                    await collection.InsertAsync(inboxId, new InboxMessage { Id = inboxId });

                    // If insert succeeds without throwing DocumentExistsException, process the message
                    await collection.UpsertAsync(basketDto.Id, basketDto);

                    _logger.LogInformation("Successfully processed and saved basket {BasketId} to Couchbase.", basketDto.BasketId);
                }
                catch (Couchbase.Core.Exceptions.KeyValue.DocumentExistsException)
                {
                    _logger.LogWarning("Message with BasketId {BasketId} already processed (Inbox pattern handled).", basketDto.BasketId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from NATS");
            }
        }
    }
}
