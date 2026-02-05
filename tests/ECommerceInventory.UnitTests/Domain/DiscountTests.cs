using ECommerceInventory.Domain.Discounts;
using FluentAssertions;

namespace ECommerceInventory.UnitTests.Domain;

public class DiscountTests
{
    [Fact]
    public void PercentageDiscountCard_CalculatesCorrectDiscount()
    {
        var card = new PercentageDiscountCard(percentage: 10, minimumAmount: 0);
        var total = 100m;

        var discount = card.CalculateDiscount(total);


        discount.Should().Be(10m);
    }

    [Fact]
    public void PercentageDiscountCard_CanApply_ReturnsTrue()
    {
        var card = new PercentageDiscountCard(percentage: 10, minimumAmount: 50);
        var total = 100m;

        var canApply = card.CanApply(total);


        canApply.Should().BeTrue();
    }

    [Fact]
    public void FixedAmountDiscountCard_AppliesDiscount_WhenAboveMinimum()
    {
        var card = new FixedAmountDiscountCard(discountAmount: 20, minimumAmount: 100);
        var total = 150m;

        var discount = card.CalculateDiscount(total);


        discount.Should().Be(20m);
    }

    [Fact]
    public void FixedAmountDiscountCard_ReturnsZero_WhenBelowMinimum()
    {
        var card = new FixedAmountDiscountCard(discountAmount: 20, minimumAmount: 100);
        var total = 50m;

        var discount = card.CalculateDiscount(total);


        discount.Should().Be(0m);
    }

    [Fact]
    public void DiscountCardFactory_CreatesPercentageCard()
    {
        var factory = new DiscountCardFactory();

        var card = factory.CreateCard("percentage", value: 15, minimumAmount: 0);


        card.Should().BeOfType<PercentageDiscountCard>();
        card.CardType.Should().Be("Percentage");
    }

    [Fact]
    public void DiscountCardFactory_CreatesFixedAmountCard()
    {
        var factory = new DiscountCardFactory();

        var card = factory.CreateCard("fixedamount", value: 25, minimumAmount: 100);


        card.Should().BeOfType<FixedAmountDiscountCard>();
        card.CardType.Should().Be("FixedAmount");
    }

    [Fact]
    public void DiscountCardFactory_ThrowsOnUnknownType()
    {
        var factory = new DiscountCardFactory();

        var act = () => factory.CreateCard("unknown", value: 10);


        act.Should().Throw<ArgumentException>()
            .WithMessage("Unknown discount card type: unknown*")
            .WithParameterName("cardType");
    }
}
