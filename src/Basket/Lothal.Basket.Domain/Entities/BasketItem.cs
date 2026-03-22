using System.ComponentModel.DataAnnotations;

namespace Lothal.Basket.Domain.Entities;

public class BasketItem
{
    [Key]
    public Guid Id { get; set; }
    public Guid BasketId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
