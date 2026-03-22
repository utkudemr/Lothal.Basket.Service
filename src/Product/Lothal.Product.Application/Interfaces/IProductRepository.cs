using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lothal.Product.Application.Interfaces;

public interface IProductRepository
{
    Task<Lothal.Product.Domain.Entities.Product?> GetByBarcodeAsync(string barcode);
    Task BulkMergeAsync(IEnumerable<Lothal.Product.Domain.Entities.Product> products);
    Task SeedDataAsync(IEnumerable<Lothal.Product.Domain.Entities.Product> products);
}
