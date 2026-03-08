using MediatR;
using Lothal.Basket.Service.Domain.Entities;

namespace Lothal.Basket.Service.Application.Queries;

public record GetBasketByIdQuery(Guid Id) : IRequest<Domain.Entities.Basket?>;
