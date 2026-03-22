using MediatR;

namespace Lothal.Product.Application.Commands;

public class ProductDto
{
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}

public class BulkMergeProductsCommand : IRequest<bool>
{
    public List<ProductDto> Products { get; set; } = new();
}
