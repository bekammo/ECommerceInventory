using System.ComponentModel.DataAnnotations;

namespace ECommerceInventory.Application.DTOs.Product;

public record UpdateProductRequest(
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 200 characters.")]
    string? Name,
    
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    string? Description,
    
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    decimal? Price,
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    int? StockQuantity);
