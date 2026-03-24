using Dapper;
using Lothal.Stock.Application.Interfaces;
using Lothal.Stock.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Lothal.Stock.Infrastructure.Data;

public class PostgresStockRepository : IStockRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresStockRepository> _logger;

    public PostgresStockRepository(IConfiguration configuration, ILogger<PostgresStockRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("StockDb")
            ?? throw new InvalidOperationException("Missing connection string 'StockDb'");
        _logger = logger;
    }

    public async Task<StockDocument?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT 
                barcode as Barcode, 
                warehouse_quantity as WarehouseQuantity, 
                source as Source, 
                last_updated_at as LastUpdatedAt
            FROM stocks
            WHERE barcode = @Barcode
            """;

        var result = await conn.QuerySingleOrDefaultAsync<StockDocument>(
            new CommandDefinition(sql, new { Barcode = barcode }, cancellationToken: ct));

        _logger.LogDebug("PostgreSQL GetByBarcode {Barcode} — Found={Found}", barcode, result is not null);
        return result;
    }

    public async Task UpsertAsync(StockDocument document, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);

        // PostgreSQL UPSERT — idempotent, safe for bulk NATS feed
        const string sql = """
            INSERT INTO stocks (barcode, warehouse_quantity, source, last_updated_at)
            VALUES (@Barcode, @WarehouseQuantity, @Source, @LastUpdatedAt)
            ON CONFLICT (barcode)
            DO UPDATE SET
                warehouse_quantity = EXCLUDED.warehouse_quantity,
                source             = EXCLUDED.source,
                last_updated_at    = EXCLUDED.last_updated_at
            """;

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                document.Barcode,
                document.WarehouseQuantity,
                document.Source,
                document.LastUpdatedAt
            }, cancellationToken: ct));

        _logger.LogDebug("PostgreSQL Upsert {Barcode} qty={Qty}", document.Barcode, document.WarehouseQuantity);
    }
}
