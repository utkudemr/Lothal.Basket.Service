namespace Lothal.Stock.Api;

public record BatchStockRequest(IReadOnlyList<string> Barcodes);
