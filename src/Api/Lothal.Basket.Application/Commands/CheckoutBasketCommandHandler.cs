using Lothal.Mediator.Core.Dispatchers;
using Lothal.Basket.Domain.Entities;
using Lothal.Basket.Domain.Repositories;
using NATS.Client.Core;
using System.Text.Json;

namespace Lothal.Basket.Application.Commands;

public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand, bool>
{
    private readonly IBasketRepository _repository;
    private readonly INatsConnection _natsConnection;

    public CheckoutBasketCommandHandler(IBasketRepository repository, INatsConnection natsConnection)
    {
        _repository = repository;
        _natsConnection = natsConnection;
    }

    public async Task<bool> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _repository.GetFromCacheAsync(request.Id, cancellationToken);
        if (basket == null) return false;

        basket.Status = BasketStatus.Completed;

        // Publish to NATS
        var payload = JsonSerializer.Serialize(basket);
        await _natsConnection.PublishAsync("baskets.checkout", payload, cancellationToken: cancellationToken);

        // Remove from cache
        await _repository.DeleteFromCacheAsync(request.Id, cancellationToken);

        return true;
    }
}
