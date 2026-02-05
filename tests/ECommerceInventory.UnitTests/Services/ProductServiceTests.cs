using ECommerceInventory.Application.DTOs.Product;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace ECommerceInventory.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _productService = new ProductService(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAll_Success()
    {
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Description = "Desc 1", Price = 10m, StockQuantity = 100, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Description = "Desc 2", Price = 20m, StockQuantity = 50, CreatedAt = DateTime.UtcNow }
        };

        _productRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        var result = await _productService.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_Found()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var result = await _productService.GetByIdAsync(productId);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(productId);
        result.Data.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetById_NotFound()
    {
        var productId = Guid.NewGuid();

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var result = await _productService.GetByIdAsync(productId);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Create_Success()
    {
        var request = new CreateProductRequest("New Product", "New Description", 49.99m, 200);

        var result = await _productService.CreateAsync(request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.Price.Should().Be(request.Price);
        result.Data.StockQuantity.Should().Be(request.StockQuantity);

        _productRepositoryMock.Verify(x => x.AddAsync(It.Is<Product>(p =>
            p.Name == request.Name &&
            p.Description == request.Description &&
            p.Price == request.Price &&
            p.StockQuantity == request.StockQuantity)), Times.Once);
    }

    [Fact]
    public async Task Update_Success()
    {
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Description",
            Price = 10m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow
        };

        var request = new UpdateProductRequest("Updated Name", "Updated Description", 19.99m, 100);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);

        var result = await _productService.UpdateAsync(productId, request);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Updated Name");
        result.Data.Description.Should().Be("Updated Description");
        result.Data.Price.Should().Be(19.99m);
        result.Data.StockQuantity.Should().Be(100);

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Product>(p =>
            p.Id == productId &&
            p.Name == "Updated Name")), Times.Once);
    }

    [Fact]
    public async Task Delete_Success()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Product to Delete",
            Description = "Description",
            Price = 10m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var result = await _productService.DeleteAsync(productId);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _productRepositoryMock.Verify(x => x.DeleteAsync(productId), Times.Once);
    }
}
