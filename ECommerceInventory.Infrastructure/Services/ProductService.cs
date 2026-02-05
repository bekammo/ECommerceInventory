using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.DTOs.Product;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Domain.Exceptions;

namespace ECommerceInventory.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;

    public ProductService(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync()
    {
        var products = await _productRepo.GetAllAsync();
        var dtos = products.Select(MapToDto);
        return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id)
    {
        Product? product = await _productRepo.GetByIdAsync(id);
        if (product is null)
        {
            return ApiResponse<ProductDto>.FailureResponse("Product not found.");
        }

        return ApiResponse<ProductDto>.SuccessResponse(MapToDto(product));
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request)
    {
        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepo.AddAsync(product);

            return ApiResponse<ProductDto>.SuccessResponse(MapToDto(product));
        }
        catch (ConcurrencyException)
        {
            return ApiResponse<ProductDto>.FailureResponse("A concurrency conflict occurred. Please try again.");
        }
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                return ApiResponse<ProductDto>.FailureResponse("Product not found.");
            }

            if (request.Name != null)
            {
                product.Name = request.Name;
            }

            if (request.Description != null)
            {
                product.Description = request.Description;
            }

            if (request.Price.HasValue)
            {
                product.Price = request.Price.Value;
            }

            if (request.StockQuantity.HasValue)
            {
                product.StockQuantity = request.StockQuantity.Value;
            }

            await _productRepo.UpdateAsync(product);

            return ApiResponse<ProductDto>.SuccessResponse(MapToDto(product));
        }
        catch (ConcurrencyException)
        {
            return ApiResponse<ProductDto>.FailureResponse("A concurrency conflict occurred. The product may have been modified by another user. Please refresh and try again.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product is null)
            {
                return ApiResponse<bool>.FailureResponse("Product not found.");
            }

            await _productRepo.DeleteAsync(id);

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (ConcurrencyException)
        {
            return ApiResponse<bool>.FailureResponse("A concurrency conflict occurred. The product may have been modified by another user. Please refresh and try again.");
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt);
    }
}
