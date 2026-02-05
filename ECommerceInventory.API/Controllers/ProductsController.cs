using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.DTOs.Product;
using ECommerceInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceInventory.API.Controllers;

/// <summary>
/// Product catalog operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductRequest request)
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<ProductDto>.FailureResponse("Authentication required."));
        }

        var result = await _productService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Updates a product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<ProductDto>.FailureResponse("Authentication required."));
        }

        var result = await _productService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Authentication required."));
        }

        var result = await _productService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
