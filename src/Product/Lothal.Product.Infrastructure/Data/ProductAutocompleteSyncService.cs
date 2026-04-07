using Lothal.Product.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Lothal.Product.Infrastructure.Data;

public class ProductAutocompleteSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductAutocompleteSyncService> _logger;

    public ProductAutocompleteSyncService(
        IServiceProvider serviceProvider, 
        ILogger<ProductAutocompleteSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            var db = redis.GetDatabase();

            var count = await db.SortedSetLengthAsync("product:autocomplete");
            if (count == 0)
            {
                _logger.LogInformation("Redis product:autocomplete is empty. Syncing from ElasticSearch...");

                var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var autocomplete = scope.ServiceProvider.GetRequiredService<IProductAutocompleteService>();

                // 2000'e kadar ürünü çekebiliriz (Şu an 1000 civarı var)
                // Gerçek ortamda scan/scroll kullanmak gerekir.
                var pagedResult = await repo.GetAllAsync(0, 5000); 

                var list = pagedResult.Items.ToList();
                if (list.Any())
                {
                    await autocomplete.IndexProductsAsync(list);
                    _logger.LogInformation("Successfully synced {Count} products to Redis Autocomplete.", list.Count);
                }
            }
            else
            {
                _logger.LogInformation("Redis product:autocomplete already initialized. (Count: {Count})", count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while syncing products to Redis.");
        }
    }
}
