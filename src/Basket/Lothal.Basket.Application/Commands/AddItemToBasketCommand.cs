using Lothal.Mediator.Core.Dispatchers;
using System.Text.Json.Serialization;

namespace Lothal.Basket.Application.Commands;

public record AddItemToBasketCommand(Guid BasketId, string Barcode, int Quantity) : IRequest<bool>;

public class ProductDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
