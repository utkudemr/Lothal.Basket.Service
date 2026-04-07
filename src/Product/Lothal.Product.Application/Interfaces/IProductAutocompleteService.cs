using Lothal.Product.Domain.Entities;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Interfaces;

public interface IProductAutocompleteService
{
    Task IndexProductsAsync(IEnumerable<ProductEntity> products);
    Task DeleteProductAsync(string barcode);
    Task<IEnumerable<ProductEntity>> SearchAsync(string query, int maxResults = 10);
}
