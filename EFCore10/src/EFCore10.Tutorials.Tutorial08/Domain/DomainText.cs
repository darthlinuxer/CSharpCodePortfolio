namespace EFCore10.Tutorials.Tutorial08.Domain;

internal static class DomainText
{
    /// <summary>
    /// Normalizes required text and enforces length limits.
    /// </summary>
    public static Result<string> Required(string? value, string label, int minLength = 1, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<string>.Failure(DomainErrors.RequiredText(label));

        var normalized = string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return normalized.Length >= minLength && normalized.Length <= maxLength
            ? Result<string>.Success(normalized)
            : Result<string>.Failure(DomainErrors.TextLength(label, minLength, maxLength));
    }
}
