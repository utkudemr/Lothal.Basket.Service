using Lothal.Basket.Service.Domain.Repositories;

namespace Lothal.Basket.Service.Infrastructure.Data;

public class BasketRepository : IBasketRepository
{
    private readonly AppDbContext _context;

    public BasketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Domain.Entities.Basket basket, CancellationToken cancellationToken = default)
    {
        _context.Baskets.Add(basket);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
