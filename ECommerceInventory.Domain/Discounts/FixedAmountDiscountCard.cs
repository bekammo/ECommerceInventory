namespace ECommerceInventory.Domain.Discounts;

public class FixedAmountDiscountCard : IDiscountCard
{
    private readonly decimal _discountAmount;
    private readonly decimal _minimumAmount;

    public string CardType => "FixedAmount";

    public FixedAmountDiscountCard(decimal discountAmount, decimal minimumAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));
        }
        
        if (minimumAmount < 0)
        {
            throw new ArgumentException("Minimum amount cannot be negative.", nameof(minimumAmount));
        }
        
        _discountAmount = discountAmount;
        _minimumAmount = minimumAmount;
    }

    public decimal CalculateDiscount(decimal total)
    {
        if (!CanApply(total))
            return 0;

        return Math.Min(_discountAmount, total);
    }

    public bool CanApply(decimal total) => total >= _minimumAmount;
}
