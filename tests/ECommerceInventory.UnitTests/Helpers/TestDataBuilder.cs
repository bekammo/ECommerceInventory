using ECommerceInventory.Application.DTOs.Order;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Domain.Enums;

namespace ECommerceInventory.UnitTests.Helpers;

public static class TestDataBuilder
{
    public static User CreateUser(
        string email = "test@test.com",
        string firstName = "Test",
        string lastName = "User",
        string passwordHash = "hashedPassword",
        string passwordSalt = "salt")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Product CreateProduct(
        string name = "Test",
        int stock = 10,
        decimal price = 99.99m,
        string desc = "Test product description")
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = desc,
            Price = price,
            StockQuantity = stock,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Order CreateOrder(
        Guid userId,
        decimal totalAmount = 100m,
        decimal discountAmount = 0m,
        OrderStatus status = OrderStatus.Pending,
        PaymentStatus paymentStatus = PaymentStatus.Pending)
    {
        var finalAmount = totalAmount - discountAmount;

        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Items = [],
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            Status = status,
            PaymentStatus = paymentStatus,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static OrderItem CreateOrderItem(
        Guid orderId,
        Guid productId,
        string productName = "Test Product",
        int quantity = 1,
        decimal unitPrice = 25m)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity
        };
    }

    public static CreateOrderRequest CreateOrderRequest(
        Guid productId,
        int quantity = 1,
        string? discountCode = null)
    {
        return new CreateOrderRequest(
            [new OrderItemRequest(productId, quantity)],
            discountCode);
    }

    public static Session CreateSession(
        Guid userId,
        string token = "test-token",
        bool isRevoked = false,
        TimeSpan? expiresIn = null)
    {
        var expiresAt = DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromDays(7));

        return new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            DeviceInfo = "Test Device",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = isRevoked
        };
    }
}
