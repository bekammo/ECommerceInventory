using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.Repositories;
using FluentAssertions;

namespace ECommerceInventory.UnitTests.Concurrency;

public class ProductRepositoryTests
{
    private readonly InMemoryProductRepository _repository;

    public ProductRepositoryTests()
    {
        _repository = new InMemoryProductRepository();
    }

    [Fact]
    public async Task Update_Succeeds_WhenVersionMatches()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Description",
            Price = 10m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(product);
        var initialVersion = BitConverter.ToInt64(product.RowVersion, 0);


        product.Name = "Updated Product";
        await _repository.UpdateAsync(product);

        var updatedProduct = await _repository.GetByIdAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Updated Product");
        BitConverter.ToInt64(updatedProduct.RowVersion, 0).Should().Be(initialVersion + 1);
    }

    [Fact]
    public async Task Update_ThrowsConcurrencyException_WhenVersionMismatch()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Description",
            Price = 10m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(product);

        var staleProduct = new Product
        {
            Id = product.Id,
            Name = "Stale Update",
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            RowVersion = BitConverter.GetBytes(0L), 
            CreatedAt = product.CreatedAt
        };

        var act = () => _repository.UpdateAsync(staleProduct);


        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Concurrency conflict*");
    }
}
