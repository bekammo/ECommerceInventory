using ECommerceInventory.Domain.Enums;

namespace ECommerceInventory.Application.DTOs.Order;

public record OrderStatusDto(
    Guid OrderId,
    OrderStatus Status,
    PaymentStatus PaymentStatus);
