using Lothal.Stock.Application.Interfaces;
using MediatR;

namespace Lothal.Stock.Application.Commands;

/// <summary>
/// Atomically reserves the requested quantity via Redis Lua script.
/// Returns ReservationResult — caller maps InsufficientStock to 409 Conflict.
/// </summary>
public record ReserveStockCommand(string Barcode, int Quantity) : IRequest<ReservationResult>;
