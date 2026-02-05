using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.BackgroundServices;
using ECommerceInventory.Infrastructure.Data;
using ECommerceInventory.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerceInventory.UnitTests.Helpers;

public static class MockSetup
{
    public static Mock<IUserRepository> CreateUserRepo(User? existingUser = null)
    {
        var mock = new Mock<IUserRepository>();

        mock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) =>
                existingUser?.Email == email ? existingUser : null);

        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) =>
                existingUser?.Id == id ? existingUser : null);

        mock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    public static Mock<IProductRepository> CreateProductRepo(List<Product>? products = null)
    {
        var mock = new Mock<IProductRepository>();
        var productList = products ?? [];

        mock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(productList);

        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => productList.FirstOrDefault(p => p.Id == id));

        mock.Setup(x => x.AddAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    public static Mock<IOrderRepository> CreateOrderRepo(List<Order>? orders = null)
    {
        var mock = new Mock<IOrderRepository>();
        var orderList = orders ?? [];

        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => orderList.FirstOrDefault(o => o.Id == id));

        mock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid userId) => orderList.Where(o => o.UserId == userId));

        mock.Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    public static Mock<ISessionRepository> CreateSessionRepo(List<Session>? sessions = null)
    {
        var mock = new Mock<ISessionRepository>();
        var sessionList = sessions ?? [];

        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => sessionList.FirstOrDefault(s => s.Id == id));

        mock.Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((string token) => sessionList.FirstOrDefault(s => s.Token == token));

        mock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid userId) => sessionList.Where(s => s.UserId == userId));

        mock.Setup(x => x.AddAsync(It.IsAny<Session>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.RevokeAllByUserIdAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        return mock;
    }

    public static Mock<IPasswordHasher> CreateHasher(
        string expectedHash = "hashedPassword",
        string expectedSalt = "salt",
        bool verifyResult = true)
    {
        var mock = new Mock<IPasswordHasher>();

        mock.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns((expectedHash, expectedSalt));

        mock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(verifyResult);

        return mock;
    }

    public static Mock<ITokenGenerator> CreateTokenGen(string token = "test-token")
    {
        var mock = new Mock<ITokenGenerator>();

        mock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(token);

        mock.Setup(x => x.ValidateToken(token))
            .Returns((string t) => t == token
                ? new TokenPayload(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(1))
                : null);

        return mock;
    }

    public static PaymentQueue CreatePaymentQueue()
    {
        return new PaymentQueue();
    }

    public static ApplicationDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
