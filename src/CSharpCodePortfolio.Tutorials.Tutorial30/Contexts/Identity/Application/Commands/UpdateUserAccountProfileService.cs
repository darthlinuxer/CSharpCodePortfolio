using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Domain.Aggregates.UserAccounts.ValueObjects;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Errors;
using CSharpCodePortfolio.Tutorials.Tutorial30.SharedKernel.Functional;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Contexts.Identity.Application.Commands;

public sealed class UpdateUserAccountProfileService(
    IUserAccountLookup lookup,
    IRepository<UserAccount, Guid> repository,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Either<Seq<DomainError>, UserAccountDto>> UpdateAsync(
        UpdateUserAccountProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = CreateProfile(request);

        return await profile.Match(
            Right: values => UpdateValidatedAsync(request.UserId, values, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, UserAccountDto>(errors))).ConfigureAwait(false);
    }

    private static Either<Seq<DomainError>, UserAccountProfile> CreateProfile(UpdateUserAccountProfileRequest request) =>
        (
            PersonName.Create(request.Name.ToNonBlankOption()),
            Email.Create(request.Email.ToNonBlankOption()),
            PhoneNumber.CreateOptional(request.PhoneNumber.ToNonBlankOption()))
        .Combine((name, email, phoneNumber) => new UserAccountProfile(name, email, phoneNumber));

    private async Task<Either<Seq<DomainError>, UserAccountDto>> UpdateValidatedAsync(
        Guid userId,
        UserAccountProfile values,
        CancellationToken cancellationToken)
    {
        var account = await repository.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);

        return await account.Match(
            Some: value => UpdateLoadedAsync(value, values, cancellationToken),
            None: () => Task.FromResult(Left<Seq<DomainError>, UserAccountDto>(
                Seq1<DomainError>(new UserAccountNotFoundError())))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, UserAccountDto>> UpdateLoadedAsync(
        UserAccount account,
        UserAccountProfile values,
        CancellationToken cancellationToken)
    {
        var emailAlreadyExists = await lookup.EmailExistsAsync(values.Email, cancellationToken).ConfigureAwait(false);
        var updated =
            from emailAvailable in account.EnsureEmailCanChangeTo(values.Email, emailAlreadyExists)
            from renamed in account.Rename(values.Name, clock)
            from emailChanged in account.ChangeEmail(values.Email, clock)
            from phoneChanged in account.ChangePhoneNumber(values.PhoneNumber, clock)
            select default(Unit);

        return await updated.Match(
            Right: _ => CommitUpdatedAsync(account, cancellationToken),
            Left: error => Task.FromResult(Left<Seq<DomainError>, UserAccountDto>(Seq1<DomainError>(error)))).ConfigureAwait(false);
    }

    private async Task<Either<Seq<DomainError>, UserAccountDto>> CommitUpdatedAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        var saved = await unitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

        return saved.Match(
            Right: _ => Right<Seq<DomainError>, UserAccountDto>(UserAccountDto.From(account)),
            Left: errors => Left<Seq<DomainError>, UserAccountDto>(errors));
    }

    private sealed record UserAccountProfile(
        PersonName Name,
        Email Email,
        Option<PhoneNumber> PhoneNumber);
}
