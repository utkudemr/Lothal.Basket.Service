using Lothal.Stock.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lothal.Stock.Application.Commands;

public class ReserveStockCommandHandler(
    IStockReservationService reservationService,
    ILogger<ReserveStockCommandHandler> logger) : IRequestHandler<ReserveStockCommand, ReservationResult>
{
    private readonly IStockReservationService _reservationService = reservationService;
    private readonly ILogger<ReserveStockCommandHandler> _logger = logger;

    public async Task<ReservationResult> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting stock reservation — Barcode={Barcode} RequestedQty={Qty}",
            request.Barcode, request.Quantity);

        var result = await _reservationService.ReserveAsync(request.Barcode, request.Quantity, cancellationToken);

        if (result.Status == ReservationStatus.Success)
        {
            _logger.LogInformation(
                "Stock reserved successfully — Barcode={Barcode} Qty={Qty} RemainingAvailable={Remaining}",
                request.Barcode, request.Quantity, result.AvailableQuantity);
        }
        else
        {
            _logger.LogWarning(
                "Stock reservation failed — Barcode={Barcode} RequestedQty={Qty} Status={Status} Available={Available}",
                request.Barcode, request.Quantity, result.Status, result.AvailableQuantity);
        }

        return result;
    }
}
