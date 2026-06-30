using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using LanguageExt;
using Microsoft.AspNetCore.Http;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Presentation.Http;

/// <summary>
/// Minimal API style adapter that translates application outcomes into HTTP results.
/// </summary>
public static class RegistrationEndpoint
{
    /// <summary>
    /// Handles a registration request without making the controller infer null states.
    /// </summary>
    public static async Task<IResult> RegisterAsync(
        RegisterUserRequest request,
        RegisterUserService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RegisterAsync(request, cancellationToken).ConfigureAwait(false);

        return ToHttpResult(result);
    }

    /// <summary>
    /// Maps Either to Created, BadRequest, or Conflict based on explicit domain errors.
    /// </summary>
    public static IResult ToHttpResult(Either<Seq<DomainError>, UserAccountDto> result)
    {
        return result.Match(
            Right: user => Results.Created($"/users/{user.Id}", ToResponse(user)),
            Left: ProblemResult.FromErrors);
    }

    /// <summary>
    /// Converts the application DTO into a simple HTTP response DTO.
    /// </summary>
    private static RegisteredUserResponse ToResponse(UserAccountDto user)
    {
        return new RegisteredUserResponse(
            user.Id,
            user.Name,
            user.Email,
            ToNullable(user.PhoneNumber));
    }

    private static string? ToNullable(Option<string> option)
    {
        foreach (var value in option)
        {
            return value;
        }

        return null;
    }
}

/// <summary>
/// HTTP response shape with nullable wire fields only at the API boundary.
/// </summary>
public sealed record RegisteredUserResponse(
    Guid Id,
    string Name,
    string Email,
    string? PhoneNumber);
