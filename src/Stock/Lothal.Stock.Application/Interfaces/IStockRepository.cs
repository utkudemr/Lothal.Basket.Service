using Lothal.Stock.Domain.Entities;

namespace Lothal.Stock.Application.Interfaces;

public interface IStockRepository
{
    Task<StockDocument?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task UpsertAsync(StockDocument document, CancellationToken ct = default);
}
