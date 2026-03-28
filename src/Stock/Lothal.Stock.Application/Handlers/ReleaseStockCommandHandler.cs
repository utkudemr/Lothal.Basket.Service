using Lothal.Stock.Application.Commands;
using Lothal.Stock.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Lothal.Stock.Application.Handlers;

/// <summary>
/// Handles the release of previously reserved stock units back to the available pool.
/// This handler is invoked when a reservation is no longer needed (e.g., basket expires or checkout fails).
/// </summary>
public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand>
{
    private readonly IStockReservationService _reservationService;

    public ReleaseStockCommandHandler(IStockReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        // Release units back into the atomic Redis pool.
        await _reservationService.ReleaseAsync(request.Barcode, request.Quantity, cancellationToken);
    }
}
