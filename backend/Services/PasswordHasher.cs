using System.Security.Cryptography;

namespace SistemaDeCompras.Services;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expectedHash = Convert.FromBase64String(hash);
        var attemptHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, Algorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(attemptHash, expectedHash);
    }
}
