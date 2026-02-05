namespace ECommerceInventory.Domain.Discounts;

public interface IDiscountCard
{
    string CardType { get; }
    decimal CalculateDiscount(decimal total);
    bool CanApply(decimal total);
}
