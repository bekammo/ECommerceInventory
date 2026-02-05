namespace ECommerceInventory.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
