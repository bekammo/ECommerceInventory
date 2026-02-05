using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.DTOs.Order;
using ECommerceInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceInventory.API.Controllers;

/// <summary>
/// Order operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<OrderDto>.FailureResponse("Authentication required."));
        }

        var result = await _orderService.CreateOrderAsync(userId.Value, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Gets all user orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetAll()
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<IEnumerable<OrderDto>>.FailureResponse("Authentication required."));
        }

        var result = await _orderService.GetUserOrdersAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<OrderDto>.FailureResponse("Authentication required."));
        }

        var result = await _orderService.GetOrderByIdAsync(userId.Value, id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets order status.
    /// </summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderStatusDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<OrderStatusDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderStatusDto>>> GetStatus(Guid id)
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        if (userId == null)
        {
            return Unauthorized(ApiResponse<OrderStatusDto>.FailureResponse("Authentication required."));
        }

        var result = await _orderService.GetOrderStatusAsync(userId.Value, id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
