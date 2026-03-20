namespace Lothal.Basket.Domain.Entities;

public class Basket
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();
    public BasketStatus Status { get; set; } = BasketStatus.Active;
    
    // Add business logic for total price calculation, adding items, removing items here if needed
    public decimal TotalPrice => Items.Sum(i => i.UnitPrice * i.Quantity);
}
