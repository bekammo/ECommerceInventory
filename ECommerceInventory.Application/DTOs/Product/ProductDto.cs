namespace ECommerceInventory.Application.DTOs.Product;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);
