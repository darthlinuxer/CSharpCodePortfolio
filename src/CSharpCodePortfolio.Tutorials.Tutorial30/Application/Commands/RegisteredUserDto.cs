using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// DTO returned by the command service after a registration command succeeds.
/// </summary>
public sealed record RegisteredUserDto(
    Guid Id,
    string Name,
    string Document,
    string Email,
    Option<string> PhoneNumber);
