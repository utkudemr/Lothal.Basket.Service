using Lothal.Stock.Domain.Entities;
using Lothal.Stock.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lothal.Stock.Application.Queries;

public class GetStocksByBarcodesQueryHandler(
    IStockRepository repository,
    ILogger<GetStocksByBarcodesQueryHandler> logger)
    : IRequestHandler<GetStocksByBarcodesQuery, IReadOnlyList<StockDocument>>
{
    public async Task<IReadOnlyList<StockDocument>> Handle(
        GetStocksByBarcodesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Barcodes is null || request.Barcodes.Count == 0)
        {
            logger.LogWarning("GetStocksByBarcodesQuery called with empty barcodes list");
            return [];
        }

        logger.LogInformation("Fetching stocks for {Count} barcodes (batch)", request.Barcodes.Count);

        return await repository.GetByBarcodesAsync(request.Barcodes, cancellationToken);
    }
}
