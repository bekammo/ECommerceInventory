namespace ECommerceInventory.Domain.Discounts;

public class PercentageDiscountCard : IDiscountCard
{
    private readonly decimal _percentage;
    private readonly decimal _minimumAmount;

    public string CardType => "Percentage";

    public PercentageDiscountCard(decimal percentage, decimal minimumAmount = 0)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentException("Percentage must be between 0 and 100.", nameof(percentage));
        }
        
        if (minimumAmount < 0)
        {
            throw new ArgumentException("Minimum amount cannot be negative.", nameof(minimumAmount));
        }
        
        _percentage = percentage;
        _minimumAmount = minimumAmount;
    }

    public decimal CalculateDiscount(decimal total)
    {
        if (!CanApply(total))
            return 0;

        return total * (_percentage / 100);
    }

    public bool CanApply(decimal total) => total >= _minimumAmount;
}
