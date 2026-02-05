using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerceInventory.Infrastructure.Security;

public class TokenGenerator : ITokenGenerator
{
    private readonly byte[] _secretKey;
    private readonly ILogger<TokenGenerator> _logger;

    public TokenGenerator(IConfiguration configuration, ILogger<TokenGenerator> logger)
    {
        var secret = configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        
        if (secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters long for security.");
        }
        
        _secretKey = Encoding.UTF8.GetBytes(secret);
        _logger = logger;
    }

    public string GenerateToken(Guid userId, Guid sessionId, DateTime expiresAt)
    {
        var payload = new TokenPayload(userId, sessionId, expiresAt);
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

        var signature = ComputeSignature(payloadBase64);

        return $"{payloadBase64}.{signature}";
    }

    public TokenPayload? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return null;
        }

        var payloadBase64 = parts[0];
        var providedSignature = parts[1];

        var expectedSignature = ComputeSignature(payloadBase64);
        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedSignature),
            Encoding.UTF8.GetBytes(expectedSignature)))
        {
            return null;
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            var payload = JsonSerializer.Deserialize<TokenPayload>(payloadJson);

            if (payload is null || payload.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            return payload;
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Invalid token format during validation.");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON payload in token.");
            return null;
        }
    }

    private string ComputeSignature(string data)
    {
        using var hmac = new HMACSHA256(_secretKey);
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(signatureBytes);
    }
}
