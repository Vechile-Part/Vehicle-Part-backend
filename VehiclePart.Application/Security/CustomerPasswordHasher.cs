using System.Security.Cryptography;
using System.Text;

namespace VehiclePart.Application.Security;

public static class CustomerPasswordHasher
{
    public static bool LooksLikeHash(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Split('.', 3).Length == 3;

    public static string HashPassword(string password)
    {
        const int iterations = 100_000;
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            32);
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        var parts = storedHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations) || iterations < 1)
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
