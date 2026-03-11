using System;

namespace Lothal.Basket.Consumer.Models;

public class InboxMessage
{
    public string Id { get; set; } = string.Empty; // Store as "inbox::{Guid}"
    public DateTime ProcessedOn { get; set; } = DateTime.UtcNow;
}

public class BasketDocument
{
    public string Id { get; set; } = string.Empty; // Store as "basket::{Guid}"
    public Guid BasketId { get; set; }
    public Guid CustomerId { get; set; }
    public List<BasketItemDocument> Items { get; set; } = new();
}

public class BasketItemDocument
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
