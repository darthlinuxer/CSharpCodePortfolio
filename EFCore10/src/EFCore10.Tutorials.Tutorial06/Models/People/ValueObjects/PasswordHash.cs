using System.Security.Cryptography;
using System.Text;
using EFCore10.Tutorials.Tutorial06.Extensions;
using Konscious.Security.Cryptography;

namespace EFCore10.Tutorials.Tutorial06.Models;

public sealed record PasswordHash
{
    private const string Algorithm = "argon2id";
    private const int Version = 19;
    private const int MemorySize = 19_456;
    private const int Iterations = 2;
    private const int DegreeOfParallelism = 1;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    private PasswordHash(string value) => Value = value;

    public string Value { get; }

    public static PasswordHash HashPassword(string password)
    {
        ValidatePassword(password);

        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = DeriveHash(password, salt, MemorySize, Iterations, DegreeOfParallelism);
        var encoded =
            $"${Algorithm}$v={Version}$m={MemorySize},t={Iterations},p={DegreeOfParallelism}" +
            $"${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";

        return new PasswordHash(encoded);
    }

    public static PasswordHash FromEncodedHash(string value)
    {
        var normalized = value.TrimRequired("Password hash");
        Parse(normalized);

        return new PasswordHash(normalized);
    }

    public bool VerifyPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var parsed = Parse(Value);
        var actualHash = DeriveHash(
            password,
            parsed.Salt,
            parsed.MemorySize,
            parsed.Iterations,
            parsed.DegreeOfParallelism);

        return CryptographicOperations.FixedTimeEquals(actualHash, parsed.Hash);
    }

    public override string ToString() => Value;

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password is required.");
    }

    private static byte[] DeriveHash(string password, byte[] salt, int memorySize, int iterations, int degreeOfParallelism)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        try
        {
            using var argon2 = new Argon2id(passwordBytes)
            {
                Salt = salt,
                MemorySize = memorySize,
                Iterations = iterations,
                DegreeOfParallelism = degreeOfParallelism
            };

            return argon2.GetBytes(HashSize);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    private static ParsedHash Parse(string encoded)
    {
        var parts = encoded.Split('$');

        if (parts is not ["", Algorithm, var version, var parameters, var salt, var hash])
            throw new DomainException("Password hash format is invalid.");

        var parsedVersion = ParsePrefixedInt(version, "v");
        var memorySize = ParseNamedParameter(parameters, "m");
        var iterations = ParseNamedParameter(parameters, "t");
        var degreeOfParallelism = ParseNamedParameter(parameters, "p");

        if (parsedVersion != Version)
            throw new DomainException("Password hash version is unsupported.");

        if (memorySize != MemorySize || iterations != Iterations || degreeOfParallelism != DegreeOfParallelism)
            throw new DomainException("Password hash parameters are unsupported.");

        var decodedSalt = DecodeBase64(salt, "salt");
        var decodedHash = DecodeBase64(hash, "hash");

        if (decodedSalt.Length != SaltSize || decodedHash.Length != HashSize)
            throw new DomainException("Password hash format is invalid.");

        return new ParsedHash(
            memorySize,
            iterations,
            degreeOfParallelism,
            decodedSalt,
            decodedHash);
    }

    private static int ParsePrefixedInt(string value, string prefix)
    {
        var expectedPrefix = $"{prefix}=";

        return value.StartsWith(expectedPrefix, StringComparison.Ordinal)
            && int.TryParse(value[expectedPrefix.Length..], out var parsed)
            && parsed > 0
                ? parsed
                : throw new DomainException("Password hash parameters are invalid.");
    }

    private static int ParseNamedParameter(string parameters, string name)
    {
        var matches = parameters
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => value.StartsWith($"{name}=", StringComparison.Ordinal))
            .ToArray();

        return matches is [var parameter]
            ? ParsePrefixedInt(parameter, name)
            : throw new DomainException("Password hash parameters are invalid.");
    }

    private static byte[] DecodeBase64(string value, string componentName)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new DomainException($"Password hash {componentName} is invalid.") { Source = exception.Source };
        }
    }

    private sealed record ParsedHash(
        int MemorySize,
        int Iterations,
        int DegreeOfParallelism,
        byte[] Salt,
        byte[] Hash);
}
