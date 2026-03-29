using System.Collections.Generic;
using MediatR;

namespace Lothal.Stock.Application.Commands;

public record StockItemIncrease(string Barcode, int Amount);
public record BulkIncreaseStockCommand(List<StockItemIncrease> Items, string TransactionId) : IRequest;
