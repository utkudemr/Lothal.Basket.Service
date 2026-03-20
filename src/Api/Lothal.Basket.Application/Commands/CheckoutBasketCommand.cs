using Lothal.Mediator.Core.Dispatchers;

namespace Lothal.Basket.Application.Commands;

public record CheckoutBasketCommand(Guid Id) : IRequest<bool>;
