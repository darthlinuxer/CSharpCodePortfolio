using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
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
    public static IResult FromErrors(Seq<DomainError> errors)
    {
        var category = errors.Fold(
            DomainErrorCategory.Validation,
            static (current, error) => current == DomainErrorCategory.Conflict
                ? current
                : error.Category);
        var (statusCode, title) = DomainErrorHttpMap.Resolve(category);

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
