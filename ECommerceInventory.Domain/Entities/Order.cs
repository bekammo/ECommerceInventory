using ECommerceInventory.Domain.Enums;

namespace ECommerceInventory.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
