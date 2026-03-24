using MediatR;

namespace Lothal.Stock.Application.Commands;

/// <summary>
/// Upserts a stock document from the ERP/WMS feed.
/// Called both by the NATS consumer (bulk feed) and the HTTP upsert endpoint (manual/test).
/// After persisting, resets the Redis available-quantity key to WarehouseQuantity.
/// </summary>
public record UpsertStockCommand(string Barcode, int WarehouseQuantity, string Source) : IRequest;
