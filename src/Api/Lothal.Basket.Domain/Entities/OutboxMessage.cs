using System;

namespace Lothal.Basket.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
}
