using Lothal.Basket.Domain.Repositories;
using Lothal.Basket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lothal.Basket.Infrastructure.Data;

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

    public async Task AddBasketAndOutboxAsync(Domain.Entities.Basket basket, OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.Baskets.Add(basket);
        _context.OutboxMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }
}
