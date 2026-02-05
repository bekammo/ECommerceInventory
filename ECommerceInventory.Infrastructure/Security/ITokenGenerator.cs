namespace ECommerceInventory.Infrastructure.Security;

public interface ITokenGenerator
{
    string GenerateToken(Guid userId, Guid sessionId, DateTime expiresAt);
    TokenPayload? ValidateToken(string token);
}

public record TokenPayload(Guid UserId, Guid SessionId, DateTime ExpiresAt);
