using PhoneNumberVo = CSharpCodePortfolio.Tutorials.Tutorial30.Domain.PhoneNumber;

namespace CSharpCodePortfolio.Tutorials.Tutorial30.Domain;

/// <summary>
/// Aggregate root for registration; required values are direct VOs and
/// optional values are explicit <see cref="Option{T}"/> VOs.
/// </summary>
/// <remarks>
/// Every behaviour method returns <c>Either&lt;Seq&lt;DomainError&gt;, T&gt;</c>
/// without a single <c>if</c> or <c>switch</c> in the domain. Composition
/// flows through LanguageExt LINQ syntax (<c>from x in y</c>) and
/// <c>Match</c> so the code reads as data flow, not as control flow.
/// </remarks>
public sealed class UserAccount : AbstractEntity<Guid>
{
    /// <summary>
    /// EF Core materialisation constructor.
    /// </summary>
    private UserAccount()
    {
    }

    /// <summary>
    /// Domain constructor used by the factory <see cref="Create"/> when all
    /// inputs validate. Marks the aggregate as created and raises the
    /// <see cref="UserAccountRegisteredDomainEvent"/>.
    /// </summary>
    private UserAccount(
        PersonName name,
        Email email,
        Option<PhoneNumber> phoneNumber,
        Timestamp registeredAtUtc)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        MarkCreated(registeredAtUtc);
        RaiseDomainEvent(new UserAccountRegisteredDomainEvent(Id, email, registeredAtUtc));
    }

    /// <summary>
    /// Gets the required non-null name.
    /// </summary>
    public PersonName Name { get; private set; } = default;

    /// <summary>
    /// Gets the required non-null email.
    /// </summary>
    public Email Email { get; private set; } = default;

    /// <summary>
    /// Gets the optional phone. The single source of truth for the
    /// aggregate's optional phone — EF Core 10 maps it through a
    /// <c>ValueConverter</c> in
    /// <c>CSharpCodePortfolio.Tutorials.Tutorial30.Infrastructure.Persistence.ConfigurationMappings.UserAccountConfiguration</c>.
    /// </summary>
    public Option<PhoneNumber> PhoneNumber { get; private set; } = None;

    /// <summary>
    /// Creates a fully valid aggregate from raw command values.
    /// </summary>
    /// <remarks>
    /// Composition is purely monadic: every step that can fail returns
    /// <c>Either&lt;Seq&lt;DomainError&gt;, _&gt;</c> and is wired through
    /// LanguageExt LINQ query syntax. There are no <c>if</c> statements in
    /// this body — control flow is encoded in the type algebra.
    /// </remarks>
    public static Either<Seq<DomainError>, UserAccount> Create(
        string? name,
        string? email,
        string? phoneNumber,
        TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return
            from userName        in PersonName.Create(name)
            from userEmail       in Email.Create(email)
            from userPhoneNumber in PhoneNumberVo.CreateOptional(phoneNumber)
            select new UserAccount(
                userName,
                userEmail,
                userPhoneNumber,
                Timestamp.UtcNow(clock));
    }

    /// <summary>
    /// Decides registration uniqueness against the application-supplied
    /// email-existence fact. Document uniqueness is no longer the
    /// aggregate's concern — that responsibility moves into the future
    /// PF (CPF) vs PJ (CNPJ) bounded context.
    /// </summary>
    public Either<Seq<DomainError>, Unit> EnsureCanBeRegistered(bool emailExists)
    {
        return emailExists
            ? Left<Seq<DomainError>, Unit>(Seq1(new UserAccountEmailDuplicateError() as DomainError))
            : Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Changes the user's required name and raises a typed domain event.
    /// Composed entirely through LanguageExt primitives — no <c>if</c>.
    /// </summary>
    public Either<Seq<DomainError>, Unit> Rename(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return Rename(PersonName.Create(value), clock);
    }

    /// <summary>
    /// Composed-overload that receives a pre-parsed <see cref="PersonName"/>.
    /// </summary>
    private Either<Seq<DomainError>, Unit> Rename(Either<Seq<DomainError>, PersonName> nextName, TimeProvider clock)
    {
        return nextName.Bind(newName => ApplyNameMutation(newName, clock));
    }

    /// <summary>
    /// Applies a valid name change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ApplyNameMutation(PersonName newName, TimeProvider clock)
    {
        var previousName = Name;
        var occurredAt = Timestamp.UtcNow(clock);

        Name = newName;
        MarkModified(occurredAt);
        RaiseDomainEvent(new UserAccountNameChangedDomainEvent(
            Id,
            previousName,
            newName,
            occurredAt));

        return Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Changes the user's required email and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> ChangeEmail(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return ChangeEmail(Email.Create(value), clock);
    }

    /// <summary>
    /// Composed-overload that receives a pre-parsed <see cref="Email"/>.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangeEmail(Either<Seq<DomainError>, Email> nextEmail, TimeProvider clock)
    {
        return nextEmail.Bind(newEmail => ApplyEmailMutation(newEmail, clock));
    }

    /// <summary>
    /// Applies a valid email change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ApplyEmailMutation(Email newEmail, TimeProvider clock)
    {
        var previousEmail = Email;
        var occurredAt = Timestamp.UtcNow(clock);

        Email = newEmail;
        MarkModified(occurredAt);
        RaiseDomainEvent(new UserAccountEmailChangedDomainEvent(
            Id,
            previousEmail,
            newEmail,
            occurredAt));

        return Right<Seq<DomainError>, Unit>(default);
    }

    /// <summary>
    /// Changes the user's optional phone and raises a typed domain event.
    /// </summary>
    public Either<Seq<DomainError>, Unit> ChangePhoneNumber(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        return ChangePhoneNumber(PhoneNumberVo.CreateOptional(value), clock);
    }

    /// <summary>
    /// Composed-overload that receives a pre-parsed <see cref="Option{PhoneNumber}"/>.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ChangePhoneNumber(Either<Seq<DomainError>, Option<PhoneNumber>> nextPhone, TimeProvider clock)
    {
        return nextPhone.Bind(newPhone => ApplyPhoneMutation(newPhone, clock));
    }

    /// <summary>
    /// Applies a valid optional phone change after the factory has parsed the raw value.
    /// </summary>
    private Either<Seq<DomainError>, Unit> ApplyPhoneMutation(Option<PhoneNumber> newPhone, TimeProvider clock)
    {
        var previousPhone = PhoneNumber;
        var occurredAt = Timestamp.UtcNow(clock);

        PhoneNumber = newPhone;
        MarkModified(occurredAt);
        RaiseDomainEvent(new UserAccountPhoneNumberChangedDomainEvent(
            Id,
            previousPhone,
            newPhone,
            occurredAt));

        return Right<Seq<DomainError>, Unit>(default);
    }
}