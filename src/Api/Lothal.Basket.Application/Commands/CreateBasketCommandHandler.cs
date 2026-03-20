using Lothal.Mediator.Core.Dispatchers;
using Lothal.Basket.Domain.Entities;
using Lothal.Basket.Domain.Repositories;

namespace Lothal.Basket.Application.Commands;

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
            Items = new List<BasketItem>(),
            Status = BasketStatus.Active
        };

        await _repository.AddToCacheAsync(basket, cancellationToken);

        return basket.Id;
    }
}
