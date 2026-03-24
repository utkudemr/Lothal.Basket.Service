using MediatR;

namespace Lothal.Stock.Application.Commands;

/// <summary>
/// Releases previously reserved units back to the available pool.
/// Should be called on basket cancellation, order failure, or timeout.
/// </summary>
public record ReleaseStockCommand(string Barcode, int Quantity) : IRequest;
