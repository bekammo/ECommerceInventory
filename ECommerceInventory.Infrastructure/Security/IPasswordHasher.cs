namespace ECommerceInventory.Infrastructure.Security;

public interface IPasswordHasher
{
    (string hash, string salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
}
