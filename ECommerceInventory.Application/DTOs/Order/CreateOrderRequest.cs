using System.ComponentModel.DataAnnotations;

namespace ECommerceInventory.Application.DTOs.Order;

public record CreateOrderRequest(
    [Required(ErrorMessage = "Order items are required.")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
    List<OrderItemRequest> Items,
    
    [StringLength(50, ErrorMessage = "Discount code cannot exceed 50 characters.")]
    string? DiscountCode);

public record OrderItemRequest(
    [Required(ErrorMessage = "Product ID is required.")]
    Guid ProductId,
    
    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000.")]
    int Quantity)
{
    // Custom validation to ensure ProductId is not empty
    public bool IsValid() => ProductId != Guid.Empty && Quantity > 0;
};
