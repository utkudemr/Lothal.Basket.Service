using MediatR;
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
            Items = new List<BasketItem>()
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "BasketCreated",
            Payload = System.Text.Json.JsonSerializer.Serialize(basket),
            OccurredOn = DateTime.UtcNow
        };

        await _repository.AddBasketAndOutboxAsync(basket, outboxMessage, cancellationToken);

        return basket.Id;
    }
}
