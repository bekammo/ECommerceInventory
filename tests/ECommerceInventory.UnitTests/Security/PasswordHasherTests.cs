using ECommerceInventory.Infrastructure.Security;
using FluentAssertions;

namespace ECommerceInventory.UnitTests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ReturnsDifferentHashForSamePassword()
    {
        var password = "SecurePassword123!";

        var (hash1, salt1) = _hasher.HashPassword(password);
        var (hash2, salt2) = _hasher.HashPassword(password);
        salt1.Should().NotBe(salt2);
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_WhenCorrect()
    {
        var password = "SecurePassword123!";
        var (hash, salt) = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash, salt);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_WhenIncorrect()
    {
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var (hash, salt) = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(wrongPassword, hash, salt);


        result.Should().BeFalse();
    }
}
