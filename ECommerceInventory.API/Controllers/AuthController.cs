using ECommerceInventory.Application.DTOs.Auth;
using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceInventory.API.Controllers;

/// <summary>
/// Authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var req = request with
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            DeviceInfo = Request.Headers.UserAgent.ToString() ?? "Unknown"
        };

        var result = await _authService.LoginAsync(req);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logs out the current session.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        var token = HttpContext.Items["Token"] as string;

        if (userId == null || string.IsNullOrEmpty(token))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Authentication required."));
        }

        var result = await _authService.LogoutAsync(userId.Value, token);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Logs out all user sessions.
    /// </summary>
    [HttpPost("logout-all")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> LogoutAll()
    {
        var userId = HttpContext.Items["UserId"] as Guid?;

        if (userId == null)
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse("Authentication required."));
        }

        var result = await _authService.LogoutAllAsync(userId.Value);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all active sessions for the current user.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SessionDto>>>> GetSessions()
    {
        var userId = HttpContext.Items["UserId"] as Guid?;
        var token = HttpContext.Items["Token"] as string;

        if (userId == null || string.IsNullOrEmpty(token))
        {
            return Unauthorized(ApiResponse<IEnumerable<SessionDto>>.FailureResponse("Authentication required."));
        }

        var result = await _authService.GetSessionsAsync(userId.Value, token);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
