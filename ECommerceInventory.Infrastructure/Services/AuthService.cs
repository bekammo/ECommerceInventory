using ECommerceInventory.Application.DTOs.Auth;
using ECommerceInventory.Application.DTOs.Common;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Security;

namespace ECommerceInventory.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ISessionRepository _sessionRepo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokenGen;
    private static readonly TimeSpan SessionDuration = TimeSpan.FromDays(7);

    public AuthService(
        IUserRepository userRepo,
        ISessionRepository sessionRepo,
        IPasswordHasher hasher,
        ITokenGenerator tokenGen)
    {
        _userRepo = userRepo;
        _sessionRepo = sessionRepo;
        _hasher = hasher;
        _tokenGen = tokenGen;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepo.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponse>.FailureResponse("Email is already registered.");
        }

        var (hash, salt) = _hasher.HashPassword(request.Password);

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = now
        };

        await _userRepo.AddAsync(user);

        var (session, token) = await CreateSessionAsync(user.Id, "Registration", "Unknown", now);

        var resp = new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            session.ExpiresAt);

        return ApiResponse<AuthResponse>.SuccessResponse(resp);
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user is null)
        {
            return ApiResponse<AuthResponse>.FailureResponse("Invalid email or password.");
        }

        if (!_hasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return ApiResponse<AuthResponse>.FailureResponse("Invalid email or password.");
        }

        var device = request.DeviceInfo ?? "Unknown Device";
        var ip = request.IpAddress ?? "Unknown";

        var (session, token) = await CreateSessionAsync(user.Id, device, ip, DateTime.UtcNow);

        var resp = new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            session.ExpiresAt);

        return ApiResponse<AuthResponse>.SuccessResponse(resp);
    }

    public async Task<ApiResponse<bool>> LogoutAsync(Guid userId, string token)
    {
        var session = await _sessionRepo.GetByTokenAsync(token);
        if (session is null || session.UserId != userId)
        {
            return ApiResponse<bool>.FailureResponse("Session not found.");
        }

        if (session.IsRevoked)
        {
            return ApiResponse<bool>.FailureResponse("Session already revoked.");
        }

        session.IsRevoked = true;
        await _sessionRepo.UpdateAsync(session);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> LogoutAllAsync(Guid userId)
    {
        await _sessionRepo.RevokeAllByUserIdAsync(userId);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsAsync(Guid userId, string currentToken)
    {
        var activeSessions = await _sessionRepo.GetActiveByUserIdAsync(userId);

        var dtos = activeSessions.Select(s => new SessionDto(
            s.Id,
            s.DeviceInfo,
            s.IpAddress,
            s.CreatedAt,
            s.ExpiresAt,
            string.Equals(s.Token, currentToken, StringComparison.Ordinal)));

        return ApiResponse<IEnumerable<SessionDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<Guid>> ValidateTokenAsync(string token)
    {
        var payload = _tokenGen.ValidateToken(token);
        if (payload is null)
        {
            return ApiResponse<Guid>.FailureResponse("Invalid or expired token.");
        }

        var session = await _sessionRepo.GetByIdAsync(payload.SessionId);
        if (session is null || session.IsRevoked)
        {
            return ApiResponse<Guid>.FailureResponse("Session not found or revoked.");
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            return ApiResponse<Guid>.FailureResponse("Session expired.");
        }

        return ApiResponse<Guid>.SuccessResponse(payload.UserId);
    }

    private async Task<(Session session, string token)> CreateSessionAsync(Guid userId, string deviceInfo, string ipAddress, DateTime now)
    {
        var sessionId = Guid.NewGuid();
        var expiresAt = now.Add(SessionDuration);
        var token = _tokenGen.GenerateToken(userId, sessionId, expiresAt);

        var session = new Session
        {
            Id = sessionId,
            UserId = userId,
            Token = token,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };

        await _sessionRepo.AddAsync(session);
        return (session, token);
    }
}
