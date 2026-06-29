# Tutorial 30 — Refactor Monadic DDD Design Document

**Status:** Draft → Approved (Camilo)
**Stack target:** .NET 10 / C# 14 / EF Core 10 / LanguageExt.Core 4.4.9
**Repository:** `CSharpCodePortfolio` → `src/CSharpCodePortfolio.Tutorials.Tutorial30`

---

## 1. Context & Goals

The current `tutorial30` implementation establishes a DDD registration scenario
with monadic types (`Option`, `Either`, `Seq`, `Fin`, `Try`). Three architectural
deficiencies remain that the user has explicitly rejected as hacks:

1. **Domain leaks**:
   - `Document` lives as a plain `string` in the aggregate, with `NormalizeDocument`
     cohabiting the aggregate root as a static side rule (no VO).
   - `AbstractEntity.CreatedBy/LastModifiedBy` typed as `Option<UserAccount>`
     couples a generic base to a concrete aggregate.
   - `DomainError.HttpStatusCode`-style routing is implemented in `ProblemResult`
     via `is ... or ...` pattern-matching on concrete domain types — the
     presentation knows the domain taxonomy (presentation→domain inversion).
   - `RegisteredUserDto.PhoneNumber` and `UserAccountQueryDto.PhoneNumber`
     expose `Option<string>` instead of wire-native `string?` — `Option` (a
     Domain concept) leaks into application DTOs and HTTP response shaping.

2. **Hacks / architectural smells**:
   - `PhoneNumberValue` is a duplicated nullable property that exists solely
     to satisfy EF Core `ComplexProperty`, while the public `PhoneNumber`
     property is `Option<PhoneNumber>` derived from it.
   - `MarkCreated/MarkModified` always receive `None` as actor — dead code.
   - Either-method overloads ship `Left<Seq<DomainError>, Unit>(OneError(error))`
     instead of leveraging monadic `Map`/`Bind` cleanup paths.
   - `IResult` wiring uses `foreach { return value; }` to extract `Option` —
     pre-`Match` style.

3. **No-monad discipline**:
   - The presentation layer uses type-discriminating `if`-style pattern matching
     on domain error subtypes, which the user has explicitly forbidden in the
     domain layer. The presentation is allowed pattern matching, but the same
     decision must surface from a domain-pure `Category` taxonomy, so the
     presentation does not grow linear branches every time a domain error is
     added.

---

## 2. Decided Design Choices (From Brainstorming)

| Q | Decision | Rationale |
|---|----------|-----------|
| Q1 | **Remove `Document` + `NormalizeDocument` entirely.** `EnsureCanBeRegistered` checks `EmailExists` only. | Decouple of "future PF vs PJ" modeling. `UserAccount` only knows required + optional identity-level fields. |
| Q2 | **Remove `CreatedBy` / `LastModifiedBy` from `AbstractEntity`.** Keep only `Option<Timestamp> CreatedAt` and `Option<Timestamp> LastModified`. | Actor audit belongs at the authentication seam, not on the entity base. |
| Q3 | **`DomainError.Category` + `DomainErrorHttpMap` table** (presentation-side static mapping). | Open/Closed: new errors extend, never modify. The Domain stays HTTP-ignorant. |
| Q4 | **Zero `if/switch` in Domain and Application layers.** Pattern matching C# allowed **only** in Presentation and Infrastructure adapters. | Rich Domain Model plus pattern matching as the official C# substitute. |

Additional architectural commitments derived from "no hacks, no breaks to
encapsulation, no duplication, production-ready":

- All VOs become **`readonly record struct`** when comparison is purely by
  value (today `Email`, `PersonName`, `PhoneNumber`, `Timestamp`, `DomainErrorCode`,
  `DomainEventType`). Avoids per-instance allocation and removes the need for
  the `private parameterless ctor()` trick that exists solely for EF.
- **`Option<PhoneNumber>` is the Domain API.** EF Core 10 maps `Option<T>`
  via `ValueConverter<Option<T>, string?>` — the mirrored `PhoneNumberValue`
  property disappears.
- **DTOs hold `string?` for nullable wire fields, not `Option<string>`.** This
  contains `Option` inside the domain.
- **`global using static LanguageExt.Prelude;` and `global using LanguageExt;`**
  in the `.csproj`'s `ItemGroup` (`<Using Include>`).
- **`TimeProvider`** is injected into the domain factory / aggregate — replaces
  `DateTime.UtcNow` for deterministic tests.
- **Either-shape cleanup**: aggregate methods return
  `Either<Seq<DomainError>, T>` where T is the new VO, not `Unit`. Single-arg
  methods `Bind` onto the previous behavior. The `Seq1(error)` sentinel for
  wrapping one error is replaced by an `ApplicativeZip` (`from ... in ...`)
  composable flow.

---

## 3. Domain Layer

### 3.1 Ubiquitous Language

| Term | Meaning |
|------|---------|
| **UserAccount** | Aggregate root representing a person capable of interacting with the platform. |
| **Registration** | The single command on the lifecycle right now: create the aggregate and commit. |
| **PersonName** | Required VO of the natural-person display name. |
| **Email** | Required, RFC-validated VO. Stable unique key for the aggregate. |
| **PhoneNumber** | Optional VO. |
| **Timestamp** | UTC-validated VO. |
| **Identity (`Id`)** | `Guid v7` generated at aggregate creation. |
| **DomainEvent** | Fact created by aggregate behavior; cleared after `SaveChanges`. |

### 3.2 Aggregate: `UserAccount`

```csharp
public sealed class UserAccount : AbstractEntity<Guid>
{
    public PersonName  Name        { get; }
    public Email       Email       { get; }
    public Option<PhoneNumber> PhoneNumber { get; }   // public, sealed

    private UserAccount(...) { /* private ctor only */ }

    public static Either<Seq<DomainError>, UserAccount> Register(
        Timestamped<PersonName> name,
        Timestamped<Email>      email,
        Timestamped<Option<PhoneNumber>> phoneNumber,
        TimeProvider             clock);

    public Either<Seq<DomainError>, UserAccount> Rename(Timestamped<PersonName> name, TimeProvider clock);
    public Either<Seq<DomainError>, UserAccount> ChangeEmail(Timestamped<Email> email, TimeProvider clock);
    public Either<Seq<DomainError>, UserAccount> ChangePhoneNumber(Timestamped<Option<PhoneNumber>> phone, TimeProvider clock);

    public Either<Seq<DomainError>, Unit> EnsureCanBeRegistered(bool emailExists);
}
```

**Invariants**

1. `Name`, `Email` are non-null at every reachable state.
2. `Email` represents the natural unique key — no `Document` exists.
3. `CreatedAt` is set exactly once, at `Register`. `LastModified` is updated
   on every successful mutation.
4. Domain events are raised only inside methods whose return value is `Right`.

### 3.3 Value Objects

```csharp
public readonly record struct PersonName(string Value);
public readonly record struct Email(string Value);
public readonly record struct PhoneNumber(string Value);
public readonly record struct Timestamp(DateTime Value);
public readonly record struct DomainErrorCode(string Value);
public readonly record struct DomainEventType(string Value);

public readonly record struct Timestamped<T>(T Value, Timestamp At);
```

Each VO provides a smart constructor that returns
`Either<Seq<DomainError>, Self>` (zero-trim/normalize logic factored in via
`Bind`). `Option<PhoneNumber>` is the standard `LanguageExt.Option<PhoneNumber>`.

### 3.4 Domain Errors with `Category`

```csharp
public abstract record DomainError(
    DomainErrorCode Code,
    string          Message,
    DomainErrorCategory Category);

public readonly record struct DomainErrorCategory(string Value)
{
    public static readonly DomainErrorCategory Validation = new("validation");
    public static readonly DomainErrorCategory Conflict    = new("conflict");
    public static readonly DomainErrorCategory NotFound    = new("not_found");
    public static readonly DomainErrorCategory Forbidden   = new("forbidden");
}
```

Concrete errors:

```csharp
public sealed record PersonNameRequiredError()         : DomainError(new Code("registration.name_required"),    "...", Validation);
public sealed record EmailInvalidError()                : DomainError(new Code("registration.email_invalid"),   "...", Validation);
public sealed record PhoneNumberInvalidError()          : DomainError(new Code("registration.phone_invalid"),  "...", Validation);
public sealed record TimestampUtcRequiredError()        : DomainError(new Code("registration.timestamp_utc"),   "...", Validation);
public sealed record UserAccountEmailDuplicateError()   : DomainError(new Code("registration.email_duplicate"),"...", Conflict);
```

`Category` is a domain concept (validation vs conflict), not an HTTP concept.
The HTTP translation lives in `DomainErrorHttpMap`.

### 3.5 Domain Events

```csharp
public interface IDomainEvent
{
    DomainEventType EventType     { get; }
    Timestamp       OccurredAtUtc { get; }
}

public sealed record UserAccountRegisteredDomainEvent(...)
    : IDomainEvent { ... }

public sealed record UserAccountNameChangedDomainEvent(...)
    : IDomainEvent { ... }

public sealed record UserAccountEmailChangedDomainEvent(...)
    : IDomainEvent { ... }

public sealed record UserAccountPhoneNumberChangedDomainEvent(...)
    : IDomainEvent { ... }
```

### 3.6 Domain Cleanup (No Hacks Allowed)

- ❌ No internal `PhoneNumberValue` mirror.
- ❌ No `DomainErrors` global catalog.
- ❌ No `if` / `switch` constructs in domain files.
- ❌ No `DateTime.UtcNow` inside domain — `TimeProvider` injected.
- ✅ `Option<T>` only for absence semantics (`PhoneNumber`).
- ✅ Every method that produces a state change uses monadic composition.

---

## 4. Application Layer

### 4.1 Application Services

`RegisterUserService` and `RenameUserService`, `ChangeEmailService`,
`ChangePhoneNumberService` are pure orchestration. They never construct a VO
without explicit factory composition:

```csharp
public sealed class RegisterUserService(
    IUserAccountLookup   lookup,
    IUserAccountWriter   writer,
    IRegistrationUnitOfWork unitOfWork,
    TimeProvider             clock)
{
    public Task<Either<Seq<DomainError>, RegisteredUserDto>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        return
            from name         in PersonName.Create(request.Name)
            from email        in Email.Create(request.Email)
            from phoneNumber  in PhoneNumber.CreateOptional(request.PhoneNumber)
            from account      in UserAccount.Register(
                                   name.At(clock.GetUtcNow()),
                                   email.At(clock.GetUtcNow()),
                                   phoneNumber.At(clock.GetUtcNow()),
                                   clock)
            from canRegister  in account.EnsureCanBeRegistered(
                                   lookup.EmailExistsAsync(account.Email, cancellationToken).Result)
            select ToDto(account).Tap(_ => writer.Add(account))
                                 .Tap(_ => unitOfWork.SaveChangesAsync(cancellationToken));
    }
}
```

(`from a in b` is the **language-ext LINQ query syntax** — it produces
`Bind`/`Select` chaining — no `if`s appear.)

### 4.2 DTOs

```csharp
public sealed record RegisterUserRequest(
    string? Name,
    string? Email,
    string? PhoneNumber);

public sealed record RegisteredUserDto(
    Guid     Id,
    string   Name,
    string   Email,
    string?  PhoneNumber);   // <-- string?, not Option<string>
```

The `Option<string>` leakage is removed. DTO = wire contract; `string?` is
its native nullable semantic.

### 4.3 Ports

```csharp
public interface IUserAccountWriter
{
    UserAccount Save(UserAccount account);
    void        Delete(UserAccount account);
}

public interface IRegistrationUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public interface IUserAccountLookup
{
    Task<bool>              EmailExistsAsync(Email email, CancellationToken ct);
    Task<Option<UserAccountQueryDto>> FindByIdAsync(Guid id, CancellationToken ct);
}
```

---

## 5. Infrastructure Layer

### 5.1 EF Core 10 Configuration

`UserAccountConfiguration` drops `PhoneNumberValue`, `CreatedBy`,
`LastModifiedBy`, `Document`. `Option<PhoneNumber>` is mapped through
**EF Core 10's built-in `ValueConverter<Option<T>, T?>` support**:

```csharp
builder.ComplexProperty(user => user.PhoneNumber, phone =>
{
    phone.Property(p => p.Value)
         .HasColumnName("PhoneNumber")
         .HasMaxLength(15)
         .IsRequired(false)
         .HasConversion(
             v => v.IsSome ? v.Value.Value : null,
             v => v is null ? Option<PhoneNumber>.None
                            : PhoneNumber.Create(v).ToOption());
});
```

`Document` mapping and `PhoneNumberValue` are gone. `CreatedAt` and
`LastModified` are mapped as complex properties.

### 5.2 DbContext

`RegistrationDbContext` adds a `DispatchDomainEvents(this DbContext)` helper
that maps the captured domain events to whatever outbox/integration
infrastructure the application registers.

---

## 6. Presentation Layer

### 6.1 HTTP Mapping

`DomainErrorHttpMap` is a static immutable table. **No type matching on
concrete errors** — the presentation reads `error.Category`:

```csharp
public static class DomainErrorHttpMap
{
    private static readonly IReadOnlyDictionary<DomainErrorCategory, (int Status, string Title)> Table =
        new Dictionary<DomainErrorCategory, (int, string)>
        {
            [DomainErrorCategory.Validation] = (400, "Invalid request."),
            [DomainErrorCategory.Conflict]    = (409, "Conflict."),
            [DomainErrorCategory.NotFound]    = (404, "Not found."),
            [DomainErrorCategory.Forbidden]   = (403, "Forbidden."),
        };

    public static (int Status, string Title) Resolve(DomainErrorCategory c) =>
        Table.TryGetValue(c, out var v)
            ? v
            : throw new InvalidOperationException($"Unknown category {c}");  // coding bug, not business bug.
}
```

### 6.2 Endpoint

```csharp
return result.Match(
    Right: dto => Results.Created($"/users/{dto.Id}", ToResponse(dto)),
    Left:  ProblemResult.FromErrors);   // pure category-driven dispatch
```

`ProblemResult.FromErrors` collapses every error in the `Seq<DomainError>`
into a single RFC 7807 response — no `if`, no pattern match on concrete types:

```csharp
public static IResult FromErrors(Seq<DomainError> errors)
{
    var (status, title) = DomainErrorHttpMap.Resolve( /* one category only */ );
    // Aggregate multiple Validation errors under 400;
    // promote to Conflict if any Conflict present (single rule, computed once).
}
```

The category promotion rule is implemented with `Seq.Fold` (not `if`).

---

## 7. Sequencing Constraints & Order of Tasks

1. VOs become `readonly record struct` (no behavior change).
2. Add `DomainErrorCategory`. Update existing errors to carry a `Category`.
3. Refactor `UserAccount` to drop `Document` and `NormalizeDocument`.
4. Drop `CreatedBy`/`LastModifiedBy` from `AbstractEntity`.
5. Drop internal `PhoneNumberValue` mirror; switch EF mapping to
   `ValueConverter<Option<PhoneNumber>, string?>`.
6. Introduce `Timestamped<T>`. Inject `TimeProvider`.
7. Replace `Seq1(error)` wrappers with monadic composition (`Map`/`Bind`).
8. Move `Option<string>` out of DTOs; convert at boundary.
9. Add `DomainErrorHttpMap` table; refactor `ProblemResult`.
10. Add `global using static LanguageExt.Prelude`.
11. Tests refactor: split monolith `LanguageExtRegistrationTests` into
    `DomainTests`, `ApplicationServiceTests`, `HttpMappingTests`,
    `InfrastructureTests`. Tests continue to encode architectural rules.

Each step has at least one failing test written first, then the change,
then a refactor pass.

---

## 8. Out of Scope

- Modeling **PF vs PJ (CPF / CNPJ)**. The user has explicitly deferred this.
- EF Core outbox / domain event publishing integration. The `EventDispatcher`
  facade is added but not bound to a transport.
- Authentication / `CreatedBy` actor. Reserved for a future tutorial.
- Full `Validation<...>` applicative style. We use `Seq<DomainError>`
  composition (consistent with current tutorial semantics).

---

## 9. Production-Grade Refinements (locked-in after Q&A with user)

### 9.1 Aggregate mutation policy

The aggregate is a **`sealed class`** (not `record class`), with
`private init`-only setters. Methods return `Either<Seq<DomainError>, UserAccount>`
without `with` re-instantiation, because:

- **EF Core tracking stability**: an aggregate that re-instantiates via
  `record.With(...)` breaks the change tracker identity and forces an
  explicit `dbContext.Update(...)` call. With the mutation pattern shown
  below, EF Core detects the change automatically (same instance, modified
  property).
- **Encapsulation total**: setters are `private init`; the aggregate is
  readable from outside but mutable only inside domain behavior.
- **Method shape**: each domain method either short-circuits via `Bind` /
  `Match` returning `this`, or returns the `Right(this)` after a `MutateX`
  helper that updates internal state and raises the event.

```csharp
public sealed class UserAccount : AbstractEntity<Guid>
{
    public PersonName  Name        { get; private init; } = default!;
    public Email       Email       { get; private init; } = default!;
    public Option<PhoneNumber> PhoneNumber { get; private init; } = None;

    public Either<Seq<DomainError>, UserAccount> Rename(
        PersonName next,
        Timestamped<PersonName> stamped)
    {
        return Name.Equals(next)
            ? Right<Seq<DomainError>, UserAccount>(this)
            : ApplyNameMutation(next, stamped.At);   // internal helper, returns this
    }

    private UserAccount ApplyNameMutation(PersonName next, Timestamp at)
    {
        Name = next;
        MarkModified(at);
        RaiseDomainEvent(new UserAccountNameChangedDomainEvent(Id, ..., at));
        return this;
    }
}
```

**Note:** the comparison-as-no-op is `Equals` (struct value equality), not
`==`. VOs are `readonly record struct`, so `Equals` is structural and
correct.

### 9.2 Time as a First-Class Citizen

- `Timestamp` becomes a `readonly record struct` (Task 1) with smart
  constructor returning `Either<Seq<DomainError>, Timestamp>`.
- `TimeProvider` is **injected** at the application / boundary layer and
  flows into domain methods via `Timestamped<T>` value objects.
- `Timestamp.UtcNow(TimeProvider clock)` is the only place where time is
  read.
- The domain never calls `DateTime.UtcNow` directly (architecture test
  enforces this).
- `Timestamped<T>(T Value, Timestamp At)` is the wrapper for "value with its
  moment". Used at the boundary to attach audit timestamps without the
  aggregate knowing about the clock itself.

### 9.3 Email uniqueness check — async-friendly boundary

The aggregate exposes a **synchronous** predicate:

```csharp
public Either<Seq<DomainError>, Unit> EnsureCanBeRegistered(bool emailExists);
```

The **application** layer composes it with the async port:

```csharp
from emailExists in lookup.EmailExistsAsync(account.Email, ct).ToAsync()
from canRegister  in account.EnsureCanBeRegistered(emailExists)
select PersistAndMap(account);
```

This keeps the domain **pure** (no `Task` inside), follows the **"functional
core, imperative shell"** pattern, and is the production-grade style.

