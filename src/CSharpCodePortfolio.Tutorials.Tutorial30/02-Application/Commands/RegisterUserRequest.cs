namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// DTO received by the application service from HTTP, tests, or console scenarios.
/// </summary>
public sealed record RegisterUserRequest(
    string? Name,
    string? Email,
    string? PhoneNumber);
