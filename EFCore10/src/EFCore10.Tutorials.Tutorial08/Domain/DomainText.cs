namespace EFCore10.Tutorials.Tutorial08.Domain;

internal static class DomainText
{
    /// <summary>
    /// Normalizes required text and enforces length limits.
    /// </summary>
    public static string Required(string? value, string label, int minLength = 1, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(DomainErrors.RequiredText, $"{label} is required.");

        var normalized = string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return normalized.Length >= minLength && normalized.Length <= maxLength
            ? normalized
            : throw new DomainException(DomainErrors.TextLength, $"{label} must have between {minLength} and {maxLength} characters.");
    }
}
