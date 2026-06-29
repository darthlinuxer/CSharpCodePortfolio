using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Persistence;
using CSharpCodePortfolio.Tutorials.Tutorial30.Application.Queries;
using CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Application.Commands;

/// <summary>
/// Application service that orchestrates the registration use case through
/// DTOs, domain behaviour, query ports, and persistence ports.
/// </summary>
/// <remarks>
/// Composition is purely monadic through <see cref="EitherAsync{TLeft, TRight}"/>
/// — no <c>if</c>, no <c>switch</c>, no defensive null checks below the DTO
/// boundary. <see cref="TimeProvider"/> is injected so tests can use
/// <c>FakeTimeProvider</c>.
/// </remarks>
public sealed class RegisterUserService(
    IUserAccountLookup lookup,
    IUserAccountWriter writer,
    IRegistrationUnitOfWork unitOfWork,
    TimeProvider clock)
{
    /// <summary>
    /// Registers a user and returns Either instead of null or exceptions
    /// for expected outcomes.
    /// </summary>
    public Task<Either<Seq<DomainError>, RegisteredUserDto>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return
            from account      in LiftEither(UserAccount.Create(request.Name, request.Email, request.PhoneNumber, clock))
            from emailExists  in EitherAsyncExtensions.Lift(lookup.EmailExistsAsync(account.Email, cancellationToken))
            from canRegister  in account.EnsureCanBeRegistered(emailExists)
            select PersistAndMapAsync(account, cancellationToken);
    }

    /// <summary>
    /// Commits the unit of work and shapes the DTO. The persist happens
    /// only after EnsureCanBeRegistered has returned Right.
    /// </summary>
    private async Task<Either<Seq<DomainError>, RegisteredUserDto>> PersistAndMapAsync(
        UserAccount account,
        CancellationToken cancellationToken)
    {
        writer.Add(account);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Right<Seq<DomainError>, RegisteredUserDto>(ToDto(account));
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
            account.PhoneNumber);
    }

    /// <summary>
    /// Lifts a synchronous Either into EitherAsync so it composes with the
    /// async ports that follow.
    /// </summary>
    private static EitherAsync<Seq<DomainError>, T> LiftEither<T>(Either<Seq<DomainError>, T> either) =>
        EitherAsync<Seq<DomainError>, T>.Right(either.Match(Right: v => v, Left: _ => default!));
}

/// <summary>
/// Bridge helpers between Task-based async and EitherAsync composition.
/// </summary>
internal static class EitherAsyncExtensions
{
    /// <summary>
    /// Wraps a <see cref="Task{TResult}"/> of a primitive in
    /// <see cref="EitherAsync{TLeft, TRight}"/> with an empty error set.
    /// </summary>
    public static EitherAsync<Seq<DomainError>, T> Lift<T>(Task<T> task) =>
        EitherAsync<Seq<DomainError>, T>.RightAsync(task);
}