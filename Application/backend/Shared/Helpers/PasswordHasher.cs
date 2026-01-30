using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace backend.Shared.Helpers;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        var hash = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);

        var actual = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
