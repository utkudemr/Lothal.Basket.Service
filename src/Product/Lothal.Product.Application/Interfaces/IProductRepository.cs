using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lothal.Product.Application.Interfaces;

public interface IProductRepository
{
    Task<Lothal.Product.Domain.Entities.Product?> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<Lothal.Product.Domain.Entities.Product>> GetAllAsync(int from, int size);
    Task BulkMergeAsync(IEnumerable<Lothal.Product.Domain.Entities.Product> products);
    Task<bool> DeleteAsync(string barcode);
    Task SeedDataAsync(IEnumerable<Lothal.Product.Domain.Entities.Product> products);
}
