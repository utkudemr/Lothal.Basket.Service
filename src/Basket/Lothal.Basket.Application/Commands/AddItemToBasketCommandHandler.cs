using Lothal.Mediator.Core.Dispatchers;
using Lothal.Basket.Domain.Entities;
using Lothal.Basket.Domain.Repositories;
using System.Text.Json;

namespace Lothal.Basket.Application.Commands;

public class AddItemToBasketCommandHandler : IRequestHandler<AddItemToBasketCommand, bool>
{
    private readonly IBasketRepository _repository;
    private readonly HttpClient _httpClient;

    public AddItemToBasketCommandHandler(IBasketRepository repository, IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _httpClient = httpClientFactory.CreateClient("ProductApi");
    }

    public async Task<bool> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _repository.GetFromCacheAsync(request.BasketId, cancellationToken);
        if (basket == null) return false; // Basket not found

        // Lookup product
        var response = await _httpClient.GetAsync($"/api/products/{request.Barcode}", cancellationToken);
        if (!response.IsSuccessStatusCode) return false; // Product not found

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (product == null) return false;

        // Check if item already exists in basket
        var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            basket.Items.Add(new BasketItem
            {
                Id = Guid.NewGuid(),
                BasketId = basket.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = request.Quantity
            });
        }

        await _repository.AddToCacheAsync(basket, cancellationToken);
        return true;
    }
}
