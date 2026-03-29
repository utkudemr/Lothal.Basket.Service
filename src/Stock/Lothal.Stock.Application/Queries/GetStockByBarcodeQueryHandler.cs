using Lothal.Stock.Domain.Entities;
using Lothal.Stock.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lothal.Stock.Application.Queries;

public class GetStockByBarcodeQueryHandler(IStockRepository repository, ILogger<GetStockByBarcodeQueryHandler> logger) : IRequestHandler<GetStockByBarcodeQuery, StockDocument?>
{
    private readonly IStockRepository _repository = repository;
    private readonly ILogger<GetStockByBarcodeQueryHandler> _logger = logger;

    public async Task<StockDocument?> Handle(GetStockByBarcodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching stock for barcode {Barcode}", request.Barcode);

        var stock = await _repository.GetByBarcodeAsync(request.Barcode, cancellationToken);

        if (stock is null)
            _logger.LogWarning("Stock not found for barcode {Barcode}", request.Barcode);

        return stock;
    }
}
