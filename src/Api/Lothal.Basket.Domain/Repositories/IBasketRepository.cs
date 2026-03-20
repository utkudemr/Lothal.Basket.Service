using Lothal.Basket.Domain.Entities;

namespace Lothal.Basket.Domain.Repositories;

public interface IBasketRepository
{
    // Active Baskets (Redis)
    Task AddToCacheAsync(Entities.Basket basket, CancellationToken cancellationToken = default);
    Task<Entities.Basket?> GetFromCacheAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteFromCacheAsync(Guid id, CancellationToken cancellationToken = default);

    // Completed Baskets (Postgres)
    Task AddToDbAsync(Entities.Basket basket, CancellationToken cancellationToken = default);
    Task<Entities.Basket?> GetFromDbAsync(Guid id, CancellationToken cancellationToken = default);

    // Unified Get (Redis then Postgres)
    Task<Entities.Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
