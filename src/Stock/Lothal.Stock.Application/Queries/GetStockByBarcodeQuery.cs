using Lothal.Stock.Domain.Entities;
using MediatR;

namespace Lothal.Stock.Application.Queries;

public record GetStockByBarcodeQuery(string Barcode) : IRequest<StockDocument?>;
