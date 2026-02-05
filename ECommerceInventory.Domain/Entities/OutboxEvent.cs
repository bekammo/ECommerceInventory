namespace ECommerceInventory.Domain.Entities;

public class OutboxEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = "";
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string Status { get; set; } = "";
}
