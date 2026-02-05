using ECommerceInventory.Domain.Enums;

namespace ECommerceInventory.Application.DTOs.Order;

public record OrderDto(
    Guid Id,
    Guid UserId,
    List<OrderItemDto> Items,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    OrderStatus Status,
    PaymentStatus PaymentStatus,
    DateTime CreatedAt);
