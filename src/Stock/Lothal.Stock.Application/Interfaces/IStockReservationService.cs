namespace Lothal.Stock.Application.Interfaces;

/// <summary>Result returned from a stock reservation attempt.</summary>
public enum ReservationStatus
{
    Success,
    InsufficientStock,
    NotFound
}

public record ReservationResult(ReservationStatus Status, int AvailableQuantity = 0);

public interface IStockReservationService
{
    /// <summary>
    /// Atomically reserves <paramref name="quantity"/> units for the given barcode.
    /// Uses a Redis Lua script — zero-contention, no distributed locks needed.
    /// </summary>
    Task<ReservationResult> ReserveAsync(string barcode, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Releases previously reserved units back to the available pool.
    /// Safe to call even if the reservation partially completed.
    /// </summary>
    Task ReleaseAsync(string barcode, int quantity, CancellationToken ct = default);

    /// <summary>
    /// Seeds (or overwrites) the Redis available-quantity key from PostgreSQL.
    /// Called during upsert to keep Redis in sync with the warehouse feed.
    /// </summary>
    Task SeedAsync(string barcode, int quantity, CancellationToken ct = default);
}
