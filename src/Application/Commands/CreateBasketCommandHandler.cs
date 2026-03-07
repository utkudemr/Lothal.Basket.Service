using MediatR;
using Lothal.Basket.Service.Domain.Entities;
using Lothal.Basket.Service.Domain.Repositories;

namespace Lothal.Basket.Service.Application.Commands;

public class CreateBasketCommandHandler : IRequestHandler<CreateBasketCommand, Guid>
{
    private readonly IBasketRepository _repository;

    public CreateBasketCommandHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = new Domain.Entities.Basket
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Items = new List<BasketItem>()
        };

        await _repository.AddAsync(basket, cancellationToken);

        return basket.Id;
    }
}
