using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.DTOs.Product;

namespace ECommerceInventory.Application.Interfaces;

public interface IProductService
{
    Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync();
    Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request);
    Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
