using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.DTOs.Order;
using ECommerceInventory.Domain.Enums;

namespace ECommerceInventory.Application.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request);
    Task<ApiResponse<IEnumerable<OrderDto>>> GetUserOrdersAsync(Guid userId);
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task<ApiResponse<OrderStatusDto>> GetOrderStatusAsync(Guid userId, Guid orderId);
    Task<ApiResponse<bool>> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status);
}
