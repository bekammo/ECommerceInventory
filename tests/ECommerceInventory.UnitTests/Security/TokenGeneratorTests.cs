using ECommerceInventory.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerceInventory.UnitTests.Security;

public class TokenGeneratorTests
{
    private readonly TokenGenerator _tokenGenerator;

    public TokenGeneratorTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "ThisIsATestSecretKeyForUnitTesting12345!"
            })
            .Build();

        var mockLogger = new Mock<ILogger<TokenGenerator>>();
        _tokenGenerator = new TokenGenerator(configuration, mockLogger.Object);
    }

    [Fact]
    public void GenerateToken_ReturnsValidFormat()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var token = _tokenGenerator.GenerateToken(userId, sessionId, expiresAt);

        token.Should().Contain(".");
        token.Split('.').Should().HaveCount(2);
    }

    [Fact]
    public void ValidateToken_ReturnsTrue_ForValidToken()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = _tokenGenerator.GenerateToken(userId, sessionId, expiresAt);

        var payload = _tokenGenerator.ValidateToken(token);


        payload.Should().NotBeNull();
        payload!.UserId.Should().Be(userId);
        payload.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void ValidateToken_ReturnsFalse_ForExpiredToken()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(-1); // Expired 1 hour ago
        var token = _tokenGenerator.GenerateToken(userId, sessionId, expiresAt);

        var payload = _tokenGenerator.ValidateToken(token);


        payload.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ReturnsFalse_ForTamperedToken()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = _tokenGenerator.GenerateToken(userId, sessionId, expiresAt);

        var parts = token.Split('.');
        var tamperedPayload = parts[0] + "tampered";
        var tamperedToken = $"{tamperedPayload}.{parts[1]}";

        var payload = _tokenGenerator.ValidateToken(tamperedToken);


        payload.Should().BeNull();
    }
}
