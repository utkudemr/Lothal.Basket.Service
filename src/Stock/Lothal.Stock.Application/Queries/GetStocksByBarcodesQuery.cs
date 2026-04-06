using Lothal.Stock.Domain.Entities;
using MediatR;

namespace Lothal.Stock.Application.Queries;

public record GetStocksByBarcodesQuery(IReadOnlyList<string> Barcodes)
    : IRequest<IReadOnlyList<StockDocument>>;
