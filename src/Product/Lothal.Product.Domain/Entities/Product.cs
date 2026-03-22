namespace Lothal.Product.Domain.Entities;

public class Product
{
    public string Id { get; set; } = string.Empty; // Mapped from Barcode usually, required by ES convention
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}
