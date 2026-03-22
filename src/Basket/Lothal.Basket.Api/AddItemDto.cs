namespace Lothal.Basket.Api;

public class AddItemDto
{
    public string Barcode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
