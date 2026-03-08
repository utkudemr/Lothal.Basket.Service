using Lothal.Basket.Service.Domain.Entities;

namespace Lothal.Basket.Service.Domain.Repositories;

public interface IBasketRepository
{
    Task AddAsync(Entities.Basket basket, CancellationToken cancellationToken = default);
    Task<Entities.Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
