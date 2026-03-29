using System.Linq;
using Lothal.Stock.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lothal.Stock.Application.Commands;

public class BulkIncreaseStockCommandHandler(
    IStockRepository repository,
    IStockReservationService reservationService,
    ILogger<BulkIncreaseStockCommandHandler> logger) : IRequestHandler<BulkIncreaseStockCommand>
{
    private readonly IStockRepository _repository = repository;
    private readonly IStockReservationService _reservationService = reservationService;
    private readonly ILogger<BulkIncreaseStockCommandHandler> _logger = logger;

    public async Task Handle(BulkIncreaseStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Idempotency Check
        var isNew = await _repository.TryRecordTransactionAsync(request.TransactionId, cancellationToken);
        if (!isNew)
        {
            _logger.LogWarning("Duplicate BulkIncreaseStockCommand ignored: {TransactionId}", request.TransactionId);
            return;
        }

        // 2. Database Update
        await _repository.BulkIncreaseAsync(request.Items, cancellationToken);

        // 3. Invalidate Redis Cache (Distributed High-Traffic Sync)
        var barcodes = request.Items.Select(x => x.Barcode);
        await _reservationService.InvalidateAsync(barcodes, cancellationToken);

        _logger.LogInformation("Stock bulk adjustment applied for {Count} barcodes. Redis cache cleared.", request.Items.Count);
    }
}
