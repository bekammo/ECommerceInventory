using ECommerceInventory.Application.DTOs.Auth;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Security;
using ECommerceInventory.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace ECommerceInventory.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Register_Success()
    {
        var request = new RegisterRequest("test@example.com", "Password123!", "John", "Doe");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
            .Returns(("hashedPassword", "salt"));

        _tokenGeneratorMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns("generated-token");

        var result = await _authService.RegisterAsync(request);


        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(request.Email);
        result.Data.FirstName.Should().Be(request.FirstName);
        result.Data.Token.Should().Be("generated-token");

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Email == request.Email &&
            u.FirstName == request.FirstName &&
            u.LastName == request.LastName)), Times.Once);

        _sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>()), Times.Once);
    }

    [Fact]
    public async Task Register_EmailExists_Fails()
    {
        var request = new RegisterRequest("existing@example.com", "Password123!", "John", "Doe");
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FirstName = "Existing",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        var result = await _authService.RegisterAsync(request);


        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already registered");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ValidCredentials_Success()
    {
        var request = new LoginRequest("test@example.com", "Password123!", "Device", "127.0.0.1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashedPassword",
            PasswordSalt = "salt",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(true);

        _tokenGeneratorMock.Setup(x => x.GenerateToken(user.Id, It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns("valid-token");

        var result = await _authService.LoginAsync(request);


        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("valid-token");
        result.Data.UserId.Should().Be(user.Id);

        _sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>()), Times.Once);
    }

    [Fact]
    public async Task Login_WrongPassword_Fails()
    {
        var request = new LoginRequest("test@example.com", "WrongPassword!", null, null);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashedPassword",
            PasswordSalt = "salt",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(false);

        var result = await _authService.LoginAsync(request);


        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid email or password");

        _sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>()), Times.Never);
    }

    [Fact]
    public async Task Login_UserNotFound_Fails()
    {
        var request = new LoginRequest("nonexistent@example.com", "Password123!", null, null);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(request);


        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Logout_Success()
    {
        var userId = Guid.NewGuid();
        var token = "valid-token";
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            DeviceInfo = "Device",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _sessionRepositoryMock.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(session);

        var result = await _authService.LogoutAsync(userId, token);


        result.Success.Should().BeTrue();
        session.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAll_RevokesAllUserSessions()
    {
        var userId = Guid.NewGuid();

        _sessionRepositoryMock.Setup(x => x.RevokeAllByUserIdAsync(userId))
            .Returns(Task.CompletedTask);

        var result = await _authService.LogoutAllAsync(userId);


        result.Success.Should().BeTrue();
        result.Message.Should().Contain("All sessions revoked");

        _sessionRepositoryMock.Verify(x => x.RevokeAllByUserIdAsync(userId), Times.Once);
    }
}
