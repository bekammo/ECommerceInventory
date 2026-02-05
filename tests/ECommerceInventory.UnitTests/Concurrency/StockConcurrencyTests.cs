using ECommerceInventory.Application.DTOs.Order;
using ECommerceInventory.Application.Interfaces.Repositories;
using ECommerceInventory.Domain.Discounts;
using ECommerceInventory.Domain.Entities;
using ECommerceInventory.Infrastructure.BackgroundServices;
using ECommerceInventory.Infrastructure.Data;
using ECommerceInventory.Infrastructure.Repositories;
using ECommerceInventory.Infrastructure.Services;
using ECommerceInventory.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerceInventory.UnitTests.Concurrency;

public class StockConcurrencyTests
{
    [Fact]
    public async Task SimultaneousPurchases_OnlySucceedUpToStockLimit()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Limited Stock Product",
            Description = "Only 3 available",
            Price = 25m,
            StockQuantity = 3,
            RowVersion = BitConverter.GetBytes(1L),
            CreatedAt = DateTime.UtcNow
        };

        var dbName = Guid.NewGuid().ToString();
        
        using (var seedContext = MockSetup.CreateInMemoryDbContext(dbName))
        {
            await seedContext.Products.AddAsync(product);
            await seedContext.SaveChangesAsync();
        }

        var paymentQueue = new PaymentQueue();
        var discountCardFactory = new DiscountCardFactory();

        var tasks = new List<Task<(bool Success, string? Message)>>();
        var barrier = new Barrier(10);

        for (int i = 0; i < 10; i++)
        {
            var userId = Guid.NewGuid();
            var request = new CreateOrderRequest(
                [new OrderItemRequest(productId, 1)],
                null);

            tasks.Add(Task.Run(async () =>
            {
                using var context = MockSetup.CreateInMemoryDbContext(dbName);
                var orderRepository = new EfOrderRepository(context);
                var productRepository = new EfProductRepository(context);
                
                var orderService = new OrderService(
                    context,
                    orderRepository,
                    productRepository,
                    paymentQueue,
                    discountCardFactory,
                    Mock.Of<ILogger<OrderService>>());

                barrier.SignalAndWait();
                var result = await orderService.CreateOrderAsync(userId, request);
                return (result.Success, result.Message);
            }));
        }

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);

        successCount.Should().Be(3, "only 3 units were in stock");
        failureCount.Should().Be(7, "7 purchases should fail due to insufficient stock");

        var failedResults = results.Where(r => !r.Success).ToList();
        failedResults.Should().AllSatisfy(r =>
            r.Message.Should().Contain("stock", "failed purchases should mention stock"));

        using var finalContext = MockSetup.CreateInMemoryDbContext(dbName);
        var finalProduct = await finalContext.Products.FindAsync(productId);
        finalProduct.Should().NotBeNull();
        finalProduct!.StockQuantity.Should().Be(0, "all 3 units should have been purchased");
    }
}
