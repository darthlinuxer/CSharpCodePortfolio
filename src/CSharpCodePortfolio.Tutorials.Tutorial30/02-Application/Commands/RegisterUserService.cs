using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain.Common.Errors;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// Orchestrates the registration use case through DTOs, domain behavior, query ports, and persistence ports.
/// </summary>
public sealed class RegisterUserService(
    IUserAccountLookup lookup,
    IUserAccountWriter writer,
    IRegistrationUnitOfWork unitOfWork,
    TimeProvider clock)
{
    /// <summary>
    /// Registers a user and returns Either instead of null or exceptions for expected outcomes.
    /// </summary>
    public async Task<Either<Seq<DomainError>, RegisteredUserDto>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = UserAccount.Create(
            ToOption(request.Name),
            ToOption(request.Email),
            ToOption(request.PhoneNumber),
            clock);

        return await account.Match(
            Right: validAccount => RegisterValidatedAsync(validAccount, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, RegisteredUserDto>(errors))).ConfigureAwait(false);
    }

    /// <summary>
    /// Collects persistence-backed facts, lets the domain decide, commits the unit of work, and returns the command DTO.
    /// </summary>
    private async Task<Either<Seq<DomainError>, RegisteredUserDto>> RegisterValidatedAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        var emailExists = await lookup.EmailExistsAsync(account.Email, cancellationToken).ConfigureAwait(false);
        var canRegister = account.EnsureCanBeRegistered(emailExists);

        return await canRegister.Match(
            Right: _ => CommitRegisteredAsync(account, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, RegisteredUserDto>(Seq1<DomainError>(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, RegisteredUserDto>> CommitRegisteredAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        writer.Add(account);
        var commit = await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return commit.Match(
            Right: _ => Right<Seq<DomainError>, RegisteredUserDto>(ToDto(account)),
            Left: errors => Left<Seq<DomainError>, RegisteredUserDto>(errors));
    }

    /// <summary>
    /// Maps the aggregate to the application DTO returned to callers.
    /// </summary>
    private static RegisteredUserDto ToDto(UserAccount account)
    {
        return new RegisteredUserDto(
            account.Id,
            account.Name.Value,
            account.Email.Value,
            account.PhoneNumber.Map(phone => phone.Value));
    }

    private static Option<string> ToOption(string? value) =>
        string.IsNullOrWhiteSpace(value) ? None : Some(value);
}
