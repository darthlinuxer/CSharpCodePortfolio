using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts.ValueObjects;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;

/// <summary>
/// Query-side DTO used when callers need to read registered users without exposing the aggregate.
/// </summary>
public sealed record UserAccountQueryDto(
    Guid Id,
    string Name,
    string Document,
    string Email,
    Option<string> PhoneNumber);

/// <summary>
/// Application query port for lookup operations backed by persistence.
/// </summary>
public interface IUserAccountLookup
{
    /// <summary>
    /// Checks whether a normalized required document is already registered.
    /// </summary>
    Task<bool> DocumentExistsAsync(string document, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a required email is already registered.
    /// </summary>
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a registered user projection by identity.
    /// </summary>
    Task<Option<UserAccountQueryDto>> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
