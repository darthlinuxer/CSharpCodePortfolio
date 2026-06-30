using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Persistence;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;

/// <summary>
/// Orchestrates the registration use case through DTOs, domain behavior, query ports, and persistence ports.
/// </summary>
public sealed class RegisterUserService(
    IUserAccountLookup lookup,
    IUserAccountWriter writer,
    ITutorial30UnitOfWork unitOfWork,
    TimeProvider clock)
{
    /// <summary>
    /// Registers a user and returns Either instead of null or exceptions for expected outcomes.
    /// </summary>
    public async Task<Either<Seq<DomainError>, UserAccountDto>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = UserAccount.Create(
            request.Name.ToNonBlankOption(),
            request.Email.ToNonBlankOption(),
            request.PhoneNumber.ToNonBlankOption(),
            clock);

        return await account.Match(
            Right: validAccount => RegisterValidatedAsync(validAccount, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, UserAccountDto>(errors))).ConfigureAwait(false);
    }

    /// <summary>
    /// Collects persistence-backed facts, lets the domain decide, commits the unit of work, and returns the command DTO.
    /// </summary>
    private async Task<Either<Seq<DomainError>, UserAccountDto>> RegisterValidatedAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        var emailExists = await lookup.EmailExistsAsync(account.Email, cancellationToken).ConfigureAwait(false);
        var canRegister = UserAccount.EnsureEmailIsAvailable(emailExists);

        return await canRegister.Match(
            Right: _ => CommitRegisteredAsync(account, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, UserAccountDto>(Seq1<DomainError>(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, UserAccountDto>> CommitRegisteredAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        writer.Add(account);
        var commit = await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return commit.Match(
            Right: _ => Right<Seq<DomainError>, UserAccountDto>(UserAccountDto.From(account)),
            Left: errors => Left<Seq<DomainError>, UserAccountDto>(errors));
    }
}
