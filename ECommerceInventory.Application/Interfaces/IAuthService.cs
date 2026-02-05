using ECommerceInventory.Application.DTOs.Auth;
using ECommerceInventory.Application.DTOs.Common;

namespace ECommerceInventory.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<bool>> LogoutAsync(Guid userId, string token);
    Task<ApiResponse<bool>> LogoutAllAsync(Guid userId);
    Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsAsync(Guid userId, string currentToken);
    Task<ApiResponse<Guid>> ValidateTokenAsync(string token);
}
