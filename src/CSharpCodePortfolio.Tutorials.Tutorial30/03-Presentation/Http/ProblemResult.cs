using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;

/// <summary>
/// Creates RFC 7807 problem results for expected application/domain errors.
/// </summary>
public static class ProblemResult
{
    /// <summary>
    /// Converts typed errors into an ASP.NET Core problem response with a compact errors extension.
    /// </summary>
    /// <remarks>
    /// The presentation layer dispatches on the domain <see cref="DomainError.Category"/>
    /// only — it never pattern-matches concrete domain error types. This keeps the
    /// presentation ignorant of domain taxonomy and lets new errors extend (Open/Closed)
    /// the catalog without touching this file.
    /// </remarks>
    public static IResult FromErrors(Seq<DomainError> errors)
    {
        var dominantCategory = errors
            .Map(error => error.Category)
            .Fold<DomainErrorCategory, DomainErrorCategory>(
                seed: DomainErrorCategory.Validation,
                folder: (current, next) => current == DomainErrorCategory.Conflict || next == DomainErrorCategory.Conflict
                    ? DomainErrorCategory.Conflict
                    : next);

        var (statusCode, title) = DomainErrorHttpMap.Resolve(dominantCategory);

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = errors.Map(error => new ProblemError(error.Code.ToString(), error.Message)).ToArray()
            });
    }
}

/// <summary>
/// ProblemDetails extension item with stable error code and human message.
/// </summary>
public sealed record ProblemError(string Code, string Message);