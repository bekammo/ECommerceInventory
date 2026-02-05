using System.Security.Cryptography;

namespace ECommerceInventory.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public (string hash, string salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            Algorithm,
            HashSize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Convert.FromBase64String(hash);

        var computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            Algorithm,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(hashBytes, computedHashBytes);
    }
}
