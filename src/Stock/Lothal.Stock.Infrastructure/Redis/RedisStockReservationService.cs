using Lothal.Stock.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Lothal.Stock.Infrastructure.Redis;

/// <summary>
/// Implements atomic stock reservation using a Redis Lua script.
/// Redis executes Lua scripts on a single thread — no race conditions, no distributed locks needed.
///
/// Key pattern : stock:{barcode}:available
/// Lua returns  : [0, remaining] on success
///                [-1, 0]        on cache miss (caller seeds from PG and retries once)
///                [-2, current]  on insufficient stock
/// </summary>
public class RedisStockReservationService : IStockReservationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IStockRepository _repository;
    private readonly ILogger<RedisStockReservationService> _logger;

    // Atomic Lua: check availability and decrement in one round-trip
    private const string ReserveLua = """
        local current = redis.call('GET', KEYS[1])
        if current == false then
            return {-1, 0}
        end
        local qty = tonumber(current)
        local requested = tonumber(ARGV[1])
        if qty < requested then
            return {-2, qty}
        end
        local remaining = tonumber(redis.call('DECRBY', KEYS[1], requested))
        return {0, remaining}
        """;

    public RedisStockReservationService(
        IConnectionMultiplexer redis,
        IStockRepository repository,
        ILogger<RedisStockReservationService> logger)
    {
        _redis = redis;
        _repository = repository;
        _logger = logger;
    }

    public async Task<ReservationResult> ReserveAsync(string barcode, int quantity, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = StockKey(barcode);

        var (status, available) = await ExecuteReserveLuaAsync(db, key, quantity);

        if (status == -1)
        {
            // Cache miss — seed from PostgreSQL then retry once
            _logger.LogInformation("Redis cache miss for {Barcode}, seeding from PostgreSQL", barcode);
            var doc = await _repository.GetByBarcodeAsync(barcode, ct);
            if (doc is null)
            {
                _logger.LogWarning("Stock document not found in PostgreSQL for {Barcode}", barcode);
                return new ReservationResult(ReservationStatus.NotFound);
            }

            await SeedAsync(barcode, doc.WarehouseQuantity, ct);
            (status, available) = await ExecuteReserveLuaAsync(db, key, quantity);
        }

        return status switch
        {
            0  => new ReservationResult(ReservationStatus.Success, available),
            -2 => new ReservationResult(ReservationStatus.InsufficientStock, available),
            _  => new ReservationResult(ReservationStatus.NotFound)
        };
    }

    public async Task ReleaseAsync(string barcode, int quantity, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var remaining = await db.StringIncrementAsync(StockKey(barcode), quantity);
        _logger.LogDebug("Redis INCRBY {Barcode} +{Qty} → remaining={Remaining}", barcode, quantity, remaining);
    }

    public async Task SeedAsync(string barcode, int quantity, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(StockKey(barcode), quantity);
        _logger.LogDebug("Redis seeded {Barcode} = {Qty}", barcode, quantity);
    }

    private async Task<(int status, int available)> ExecuteReserveLuaAsync(IDatabase db, RedisKey key, int quantity)
    {
        var result = (RedisResult[]?)await db.ScriptEvaluateAsync(ReserveLua, new RedisKey[] { key }, new RedisValue[] { quantity });
        if (result == null || result.Length < 2) return (-1, 0);

        return ((int)result[0], (int)result[1]);
    }

    private static RedisKey StockKey(string barcode) => $"stock:{barcode}:available";
}
