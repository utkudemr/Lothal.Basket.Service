using Lothal.Basket.Domain.Entities;

namespace Lothal.Basket.Domain.Repositories;

public interface IBasketRepository
{
    Task AddAsync(Entities.Basket basket, CancellationToken cancellationToken = default);
    Task AddBasketAndOutboxAsync(Entities.Basket basket, OutboxMessage message, CancellationToken cancellationToken = default);
    Task<Entities.Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
