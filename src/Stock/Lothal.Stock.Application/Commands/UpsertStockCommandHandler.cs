using Lothal.Stock.Application.Interfaces;
using Lothal.Stock.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lothal.Stock.Application.Commands;

public class UpsertStockCommandHandler : IRequestHandler<UpsertStockCommand>
{
    private readonly IStockRepository _repository;
    private readonly IStockReservationService _reservationService;
    private readonly ILogger<UpsertStockCommandHandler> _logger;

    public UpsertStockCommandHandler(
        IStockRepository repository,
        IStockReservationService reservationService,
        ILogger<UpsertStockCommandHandler> logger)
    {
        _repository = repository;
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Handle(UpsertStockCommand request, CancellationToken cancellationToken)
    {
        // Basic barcode format guard — don't call ES, just reject obviously malformed data
        if (string.IsNullOrWhiteSpace(request.Barcode) || request.WarehouseQuantity < 0)
        {
            _logger.LogWarning(
                "Skipping invalid stock upsert — Barcode={Barcode} Quantity={Qty}",
                request.Barcode, request.WarehouseQuantity);
            return;
        }

        var document = new StockDocument
        {
            Barcode = request.Barcode,
            WarehouseQuantity = request.WarehouseQuantity,
            Source = request.Source,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        // 1. Persist to PostgreSQL (source of truth for full document)
        await _repository.UpsertAsync(document, cancellationToken);

        // 2. Overwrite Redis available-qty key so reservations stay consistent
        //    with the latest warehouse quantity from the feed.
        await _reservationService.SeedAsync(request.Barcode, request.WarehouseQuantity, cancellationToken);

        _logger.LogInformation(
            "Stock upserted — Barcode={Barcode} WarehouseQty={Qty} Source={Source}",
            request.Barcode, request.WarehouseQuantity, request.Source);
    }
}
