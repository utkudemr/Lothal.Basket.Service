using Lothal.Stock.Application.Commands;
using Lothal.Stock.Domain.Entities;

namespace Lothal.Stock.Application.Interfaces;

public interface IStockRepository
{
    Task<StockDocument?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<IReadOnlyList<StockDocument>> GetByBarcodesAsync(IReadOnlyList<string> barcodes, CancellationToken ct = default);
    Task UpsertAsync(StockDocument document, CancellationToken ct = default);
    Task<bool> TryRecordTransactionAsync(string transactionId, CancellationToken ct = default);
    Task BulkIncreaseAsync(List<StockItemIncrease> items, CancellationToken ct = default);
}
