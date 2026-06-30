using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;

/// <summary>
/// DTO returned by the command service after a registration command succeeds.
/// </summary>
public sealed record RegisteredUserDto(
    Guid Id,
    string Name,
    string Email,
    Option<string> PhoneNumber);
