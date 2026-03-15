using Lothal.Mediator.Core.Dispatchers;

namespace Lothal.Basket.Application.Commands;

public record CreateBasketCommand(string CustomerId) : IRequest<Guid>;
