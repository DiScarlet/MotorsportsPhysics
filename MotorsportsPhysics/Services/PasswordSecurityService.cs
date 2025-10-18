using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace MotorsportsPhysics.Services;

public class PasswordSecurityService
{
    private const int SaltSize = 16;
    private const int Iterations = 350_000;
    private const int KeySize = 64;

    public async Task<(string Hash, string Salt)> HashWithSaltAndPepperAsync(string password)
    {
        var salt = GenerateSalt();
        var pepper = await GetPepperAsync();
        var saltPepper = salt + pepper;

        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(saltPepper),
            Iterations,
            HashAlgorithmName.SHA512,
            KeySize);

        var hash = Convert.ToBase64String(hashBytes);
        return (hash, salt);
    }

    public async Task<bool> VerifyAsync(string password, string salt, string expectedBase64Hash)
    {
        if (string.IsNullOrEmpty(expectedBase64Hash)) return false;
        var pepper = await GetPepperAsync();
        var saltPepper = salt + pepper;

        var derived = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(saltPepper),
            Iterations,
            HashAlgorithmName.SHA512,
            KeySize);

        Console.WriteLine($"[DEBUG] VerifyAsync password: der '{Convert.ToBase64String(derived)}' exp {expectedBase64Hash}");

        var expectedBytes = Convert.FromBase64String(expectedBase64Hash);
        return FixedTimeEquals(derived, expectedBytes);
    }

    private static bool FixedTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }

    private static string GenerateSalt()
    {
        Span<byte> salt = stackalloc byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return Convert.ToBase64String(salt);
    }

    private static Task<string> GetPepperAsync()
    {
        // Read from environment variable. Use USER-SECRETS or CI secrets for development/deploy.
        var pepper = Environment.GetEnvironmentVariable("PasswordPepper");
        if (string.IsNullOrWhiteSpace(pepper))
        {
            throw new InvalidOperationException("Missing environment variable 'PasswordPepper'. Set it before running the app.");
        }
        return Task.FromResult(pepper);
    }
}
