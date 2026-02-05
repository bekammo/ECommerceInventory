namespace ECommerceInventory.Application.DTOs.Order;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
