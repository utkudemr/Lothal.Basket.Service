using MediatR;

namespace Lothal.Stock.Application.Commands;

public record BulkIncreaseStockCommand(int Amount, string TransactionId) : IRequest;
