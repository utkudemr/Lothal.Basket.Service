using Lothal.Product.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Lothal.Product.Infrastructure.Data;

public class ProductSeederService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ProductSeederService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var seedProducts = new List<Domain.Entities.Product>
        {
            new Domain.Entities.Product { Barcode = "P1001", Name = "T-Shirt", Class = "Apparel", Color = "Red", Size = "M", Price = 19.99m },
            new Domain.Entities.Product { Barcode = "P1002", Name = "Jeans", Class = "Apparel", Color = "Blue", Size = "32", Price = 49.99m },
            new Domain.Entities.Product { Barcode = "P1003", Name = "Sneakers", Class = "Footwear", Color = "White", Size = "42", Price = 89.99m }
        };

        await repo.SeedDataAsync(seedProducts);
    }
}
