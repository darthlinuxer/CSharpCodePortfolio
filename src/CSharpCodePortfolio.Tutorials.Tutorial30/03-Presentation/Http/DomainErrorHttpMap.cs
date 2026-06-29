using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;

/// <summary>
/// Static table that maps a domain <see cref="DomainErrorCategory"/> to an
/// HTTP status code and a human title. New domain errors are added in the
/// domain layer; mapping entries live here and grow with the catalog.
/// </summary>
/// <remarks>
/// This is the single place where the presentation layer translates domain
/// semantics into HTTP semantics. Adding a new <see cref="DomainErrorCategory"/>
/// requires updating this table (Open/Closed compliance). Adding a new
/// concrete <see cref="DomainError"/> does NOT require any change here, as
/// long as it uses a canonical category.
/// </remarks>
public static class DomainErrorHttpMap
{
    private static readonly IReadOnlyDictionary<DomainErrorCategory, (int Status, string Title)> Table =
        new Dictionary<DomainErrorCategory, (int, string)>
        {
            [DomainErrorCategory.Validation] = (400, "Invalid request."),
            [DomainErrorCategory.Conflict]    = (409, "Conflict."),
        };

    /// <summary>
    /// Resolves the HTTP status code and title for a given domain error
    /// category. Unknown categories are treated as a programming error:
    /// they indicate a missing entry in the table, never a user mistake.
    /// </summary>
    public static (int Status, string Title) Resolve(DomainErrorCategory category)
    {
        if (Table.TryGetValue(category, out var entry))
        {
            return entry;
        }

        throw new InvalidOperationException(
            $"No HTTP mapping registered for domain error category '{category.Value}'. "
            + "Update DomainErrorHttpMap.Table to add a new entry.");
    }
}