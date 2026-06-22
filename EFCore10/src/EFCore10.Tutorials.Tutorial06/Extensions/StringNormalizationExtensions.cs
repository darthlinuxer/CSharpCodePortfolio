using EFCore10.Tutorials.Tutorial06.Models;

namespace EFCore10.Tutorials.Tutorial06.Extensions;

internal static class StringNormalizationExtensions
{
    public static string TrimRequired(this string? value, string valueName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{valueName} is required.");

        return value.Trim();
    }

    public static string NormalizeLength(this string? value, string valueName, int minLength, int maxLength)
    {
        var normalized = value.TrimRequired(valueName);

        return normalized.Length < minLength || normalized.Length > maxLength
            ? throw new DomainException($"{valueName} must have between {minLength} and {maxLength} characters.")
            : normalized;
    }

    public static string ToLowerInvariantRequired(this string? value, string valueName) =>
        value.TrimRequired(valueName).ToLowerInvariant();

    public static string ToUpperInvariantRequired(this string? value, string valueName) =>
        value.TrimRequired(valueName).ToUpperInvariant();

    public static string OnlyDigits(this string? value, string valueName) =>
        new(value.TrimRequired(valueName).Where(char.IsDigit).ToArray());
}
