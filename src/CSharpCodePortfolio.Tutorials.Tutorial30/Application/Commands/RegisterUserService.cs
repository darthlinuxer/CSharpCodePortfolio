using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;
using LanguageExt;
using static LanguageExt.Prelude;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// Orchestrates the registration use case through DTOs, domain behavior, query ports, and persistence ports.
/// </summary>
public sealed class RegisterUserService(IUserAccountLookup lookup, IUserAccountWriter writer)
{
    /// <summary>
    /// Registers a user and returns Either instead of null or exceptions for expected outcomes.
    /// </summary>
    public async Task<Either<Seq<DomainError>, RegisteredUserDto>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = UserAccount.Create(request.Name, request.Document, request.Email, request.PhoneNumber);

        return await account.Match(
            Right: validAccount => RegisterValidatedAsync(validAccount, cancellationToken),
            Left: errors => Task.FromResult(Left<Seq<DomainError>, RegisteredUserDto>(errors))).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks persistence-backed uniqueness, saves the aggregate, and returns the command DTO.
    /// </summary>
    private async Task<Either<Seq<DomainError>, RegisteredUserDto>> RegisterValidatedAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        if (await lookup.DocumentExistsAsync(account.Document, cancellationToken).ConfigureAwait(false))
            return Left<Seq<DomainError>, RegisteredUserDto>(OneError(DomainErrors.DocumentDuplicate));

        if (await lookup.EmailExistsAsync(account.Email, cancellationToken).ConfigureAwait(false))
            return Left<Seq<DomainError>, RegisteredUserDto>(OneError(DomainErrors.EmailDuplicate));

        await writer.AddAsync(account, cancellationToken).ConfigureAwait(false);

        return Right<Seq<DomainError>, RegisteredUserDto>(ToDto(account));
    }

    /// <summary>
    /// Wraps one expected error in Seq so Either keeps a consistent left type.
    /// </summary>
    private static Seq<DomainError> OneError(DomainError error) => Seq1(error);

    /// <summary>
    /// Maps the aggregate to the application DTO returned to callers.
    /// </summary>
    private static RegisteredUserDto ToDto(UserAccount account)
    {
        return new RegisteredUserDto(
            account.Id,
            account.Name.Value,
            account.Document,
            account.Email.Value,
            account.PhoneNumber.Map(phone => phone.Value));
    }
}
