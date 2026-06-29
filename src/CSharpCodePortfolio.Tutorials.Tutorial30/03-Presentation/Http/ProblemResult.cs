using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.Errors;
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
        var statusCode = IsConflict(errors)
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;

        return Results.Problem(
            statusCode: statusCode,
            title: statusCode == StatusCodes.Status409Conflict
                ? "Conflito de cadastro."
                : "Cadastro inválido.",
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = errors.Map(error => new ProblemError(error.Code.ToString(), error.Message)).ToArray()
            });
    }

    /// <summary>
    /// Detects errors that should become HTTP 409 instead of HTTP 400.
    /// </summary>
    private static bool IsConflict(Seq<DomainError> errors)
    {
        return errors.Exists(error =>
            error is UserAccountDocumentDuplicateError or UserAccountEmailDuplicateError);
    }
}

/// <summary>
/// ProblemDetails extension item with stable error code and human message.
/// </summary>
public sealed record ProblemError(string Code, string Message);
