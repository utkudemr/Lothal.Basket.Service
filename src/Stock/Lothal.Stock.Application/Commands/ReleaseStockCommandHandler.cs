using Lothal.Stock.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Lothal.Stock.Application.Commands;

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand>
{
    private readonly IStockReservationService _reservationService;
    private readonly ILogger<ReleaseStockCommandHandler> _logger;

    public ReleaseStockCommandHandler(
        IStockReservationService reservationService,
        ILogger<ReleaseStockCommandHandler> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        await _reservationService.ReleaseAsync(request.Barcode, request.Quantity, cancellationToken);

        _logger.LogInformation(
            "Stock released — Barcode={Barcode} ReleasedQty={Qty}",
            request.Barcode, request.Quantity);
    }
}
