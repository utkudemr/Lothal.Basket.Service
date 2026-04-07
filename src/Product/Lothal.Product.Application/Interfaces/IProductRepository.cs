using Lothal.BuildingBlocks.Common;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Interfaces;

public interface IProductRepository
{
    Task<ProductEntity?> GetByBarcodeAsync(string barcode);
    Task<PagedResult<ProductEntity>> GetAllAsync(int from, int size);
    Task BulkMergeAsync(IEnumerable<ProductEntity> products);
    Task<bool> DeleteAsync(string barcode);
    Task SeedDataAsync(IEnumerable<ProductEntity> products);
    Task<IEnumerable<ProductEntity>> SearchByNameAsync(string query, int size = 10);
}
