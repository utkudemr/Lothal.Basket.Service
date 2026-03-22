using Lothal.Mediator.Core.Dispatchers;
using Lothal.Basket.Domain.Entities;

namespace Lothal.Basket.Application.Queries;

public record GetBasketByIdQuery(Guid Id) : IRequest<Domain.Entities.Basket?>;
