namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;

public sealed record UpdateUserAccountProfileRequest(
    Guid UserId,
    string? Name,
    string? Email,
    string? PhoneNumber);
