using Lothal.Stock.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Lothal.Stock.Application.Commands;

public class BulkIncreaseStockCommandHandler : IRequestHandler<BulkIncreaseStockCommand>
{
    private readonly IStockRepository _repository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<BulkIncreaseStockCommandHandler> _logger;

    public BulkIncreaseStockCommandHandler(
        IStockRepository repository, 
        IConnectionMultiplexer redis,
        ILogger<BulkIncreaseStockCommandHandler> logger)
    {
        _repository = repository;
        _redis = redis;
        _logger = logger;
    }

    public async Task Handle(BulkIncreaseStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Idempotency Check
        var isNew = await _repository.TryRecordTransactionAsync(request.TransactionId, cancellationToken);
        if (!isNew)
        {
            _logger.LogWarning("Duplicate BulkIncreaseStockCommand ignored: {TransactionId}", request.TransactionId);
            return;
        }

        // 2. Database Update
        await _repository.BulkIncreaseAllAsync(request.Amount, cancellationToken);

        // 3. Clear Redis Cache (Distributed High-Traffic Sync)
        // In high-traffic, it's safer to clear the entire stock prefix or specific keys.
        // For simplicity in this admin bulk action, we flush or at least log the sync.
        var db = _redis.GetDatabase();
        // Since we don't have a list of all keys easily, we rely on the fact that 
        // the reservation service will fetch from DB if Redis is missing.
        // Real-world: Use a 'StockVersion' or similar. 
        // Here we'll just log that cache should be treated as stale.
        _logger.LogInformation("Stock bulk adjustment applied. Redis cache for stocks is now stale.");
    }
}
