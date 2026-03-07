using MediatR;

namespace Lothal.Basket.Service.Application.Commands;

public record CreateBasketCommand(string CustomerId) : IRequest<Guid>;
