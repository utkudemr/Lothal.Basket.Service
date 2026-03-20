using Lothal.Basket.Domain.Repositories;
using Lothal.Basket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using Polly;

namespace Lothal.Basket.Infrastructure.Data;

public class BasketRepository : IBasketRepository
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly IAsyncPolicy _resiliencePolicy;

    public BasketRepository(AppDbContext context, IConnectionMultiplexer redis, IAsyncPolicy resiliencePolicy)
    {
        _context = context;
        _redis = redis;
        _database = _redis.GetDatabase();
        _resiliencePolicy = resiliencePolicy;
    }

    public async Task AddToCacheAsync(Domain.Entities.Basket basket, CancellationToken cancellationToken = default)
    {
        await _resiliencePolicy.ExecuteAsync(async () =>
        {
            var data = JsonSerializer.Serialize(basket);
            await _database.StringSetAsync(basket.Id.ToString(), data, TimeSpan.FromDays(30));
        });
    }

    public async Task<Domain.Entities.Basket?> GetFromCacheAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _resiliencePolicy.ExecuteAsync(async () =>
        {
            var data = await _database.StringGetAsync(id.ToString());
            if (data.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<Domain.Entities.Basket>(data!);
        });
    }

    public async Task DeleteFromCacheAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _resiliencePolicy.ExecuteAsync(async () =>
        {
            await _database.KeyDeleteAsync(id.ToString());
        });
    }

    public async Task AddToDbAsync(Domain.Entities.Basket basket, CancellationToken cancellationToken = default)
    {
        await _resiliencePolicy.ExecuteAsync(async () =>
        {
            _context.Baskets.Add(basket);
            await _context.SaveChangesAsync(cancellationToken);
        });
    }

    public async Task<Domain.Entities.Basket?> GetFromDbAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _resiliencePolicy.ExecuteAsync(async () =>
        {
            return await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        });
    }

    public async Task<Domain.Entities.Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var basket = await GetFromCacheAsync(id, cancellationToken);
        if (basket != null) return basket;

        return await GetFromDbAsync(id, cancellationToken);
    }
}
