namespace ECommerceInventory.Domain.Discounts;

public class DiscountCardFactory
{
    public IDiscountCard CreateCard(string cardType, decimal value, decimal minimumAmount = 0)
    {
        return cardType.ToLowerInvariant() switch
        {
            "percentage" => new PercentageDiscountCard(value, minimumAmount),
            "fixedamount" => new FixedAmountDiscountCard(value, minimumAmount),
            _ => throw new ArgumentException($"Unknown discount card type: {cardType}", nameof(cardType))
        };
    }
}
