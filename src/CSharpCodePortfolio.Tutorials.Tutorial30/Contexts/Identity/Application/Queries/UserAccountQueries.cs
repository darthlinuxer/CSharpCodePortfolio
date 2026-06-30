using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using LanguageExt;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Queries;

/// <summary>
/// Query-side DTO used when callers need to read registered users without exposing the aggregate.
/// </summary>
public sealed record UserAccountQueryDto(
    Guid Id,
    string Name,
    string Email,
    Option<string> PhoneNumber);

/// <summary>
/// Application query port for lookup operations backed by persistence.
/// </summary>
public interface IUserAccountLookup
{
    /// <summary>
    /// Checks whether a required email is already registered.
    /// </summary>
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a registered user projection by identity.
    /// </summary>
    Task<Option<UserAccountQueryDto>> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
