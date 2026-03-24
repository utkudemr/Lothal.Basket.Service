namespace Lothal.Stock.Domain.Entities;

/// <summary>
/// Represents the persistent stock record for a product identified by its barcode.
/// Warehouse quantity comes from the external ERP/WMS feed (NATS stock.upsert).
/// Available quantity is tracked atomically in Redis; this field reflects the last
/// known value synced from Redis (informational only — Redis is the source of truth
/// for availability checks).
/// </summary>
public class StockDocument
{
    /// <summary>Primary key — the product barcode.</summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>Total units reported by the ERP/WMS feed.</summary>
    public int WarehouseQuantity { get; set; }

    /// <summary>Source system identifier (e.g. "ERP", "WMS", "MANUAL").</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>When this record was last updated by the feed.</summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}
