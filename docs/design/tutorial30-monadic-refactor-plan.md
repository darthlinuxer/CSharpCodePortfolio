# Tutorial 30 — Monadic DDD Refactor · Execution Plan

**Worktree:** `/tmp/tutorial30-monadic` (branch `refactor/tutorial30-monadic`)
**Skill:** `clawgator-superpowers` · `prd-to-ddd-design`
**Discipline:** TDD (RED → GREEN → REFACTOR) · Zero `if`/`switch` in Domain & Application · Open/Closed on errors · No domain leaks · No internal mirror properties · No EF-tricks via private setters.

Total tasks: **14**, ordered. Each task lists: failing test first, production code second, tests+refactor last. Subagents execute one task each in fresh sessions.

---

## Task 0 — `TimeProvider` introduction
**Goal:** Remove `DateTime.UtcNow` from `Timestamp.UtcNow`; inject `TimeProvider`.

1. **RED** `tests/.../TimestampTests.cs::Create_UsesTimeProviderForUtcNow`:
   ```csharp
   var clock = new FakeTimeProvider(...);
   var ts    = Timestamp.UtcNow(clock);
   Assert.AreEqual(clock.GetUtcNow().UtcDateTime, ts.Value);
   ```
   → fails because current signature is parameterless.

2. **GREEN** change `Timestamp.UtcNow` to `UtcNow(TimeProvider clock)`. Update
   the single call site (`UserAccount.Create`/`Rename`...).

3. **REFACTOR** extract `MarkOccurredAt(clock)` helper.

---

## Task 1 — `readonly record struct` VOs
**Goal:** Replace `sealed record class PersonName { string Value; ... }` with
`readonly record struct PersonName(string Value)`.

1. **RED** Run existing tests — they will still pass on record-class VOs.
   Add `PersonNameTests.Equals_IsStructuralWithoutReferenceIdentity` and
   `Equals_OnValueObjects_ComparesByValue` to lock the structural semantic.

2. **GREEN** Convert each VO file to `readonly record struct`. Add the smart
   factory inside the struct via a static method that still returns
   `Either<Seq<DomainError>, Self>`.

3. **REFACTOR** Drop the private parameterless constructors everywhere.

---

## Task 2 — `DomainErrorCode` & `DomainEventType` as record struct
Already record struct — confirm and add tests for type-level usage in tests.
Skipped if existing.

---

## Task 3 — `DomainError.Category`
**Goal:** Add `DomainErrorCategory` to make every error categorization
explicit. Surface to Presentation through the map, not type-switching.

1. **RED** `DomainErrorTests.AllConcreteErrors_CarryACategory` — verify that
   every concrete subclass does not return null `Category`. Will fail today
   because no `Category` exists.

2. **GREEN**
   - Add `DomainErrorCategory` `readonly record struct`.
   - Add `abstract DomainErrorCategory Category { get; }` to `DomainError`.
   - Add `Validation` and `Conflict` static instances (avoid magic strings).
   - Update each concrete error.

3. **REFACTOR** move constructor args to positional `(Code, Message, Category)`.

---

## Task 4 — Remove `Document` and `NormalizeDocument`
**Goal:** `UserAccount` no longer carries document.

1. **RED** `UserAccountTests.Register_DoesNotAcceptDocument` — test that
   `Register(...)` signature has no document parameter.

2. **GREEN** Drop `Document` property, drop `Document` ctor parameter, drop
   `NormalizeDocument` static. Update tests that referenced document.

3. **REFACTOR** Move the `EnsureCanBeRegistered` boolean helper:
   ```csharp
   public Either<Seq<DomainError>, Unit> EnsureCanBeRegistered(bool emailExists);
   ```
   Removes the now-unused `DocumentDuplicateError`.

---

## Task 5 — Drop `CreatedBy`/`LastModifiedBy` from `AbstractEntity`
**Goal:** Audit fields reduced to timestamps only.

1. **RED** `AbstractEntityTests.DoesNotExposeCreatedByOrLastModifiedBy`.

2. **GREEN** Remove the `_createdBy` and `_lastModifiedBy` fields, properties,
   `MarkCreated(...)`/`MarkModified(...)` overloads that took actor
   parameter; keep only `Timestamp` overloads.

3. **REFACTOR** Domain events no longer carry actor name (none did).

---

## Task 6 — Replace the `PhoneNumberValue` mirror
**Goal:** Eliminate the internal nullable mirror, EF maps `Option<PhoneNumber>`
   via `ValueConverter`.

1. **RED** `UserAccountTests.PhoneNumber_NoInternalMirrorPropertyExists` —
   uses reflection to assert the only existing public property is
   `PhoneNumber` of type `Option<PhoneNumber>`.

2. **GREEN** Delete the `PhoneNumberValue` property. Replace the setter/
   getter logic in `ChangePhoneNumber` with a non-mutating copy pattern
   (because VOs are immutable structs, re-creating the aggregate is the
   idiomatic monadic way; alternatively a `private readonly` field + `With`).

3. **REFACTOR + Infra**: `UserAccountConfiguration.ComplexProperty` for the
   `Option<PhoneNumber>` uses `HasConversion` from
   `Option<PhoneNumber> ↔ string?`.

---

## Task 7 — `Timestamped<T>` for monadic event timestamping
**Goal:** Make timestamps flow through the LINQ query syntax at the boundary.

1. **RED** `TimestampedTests.Combine_BindsThroughTimestamps`:
   ```csharp
   var stamp = clock.GetUtcNow();
   var result =
       from n in PersonName.Create("Ada")
       from e in Email.Create("a@b.co")
       select Timestamped.Create(n, e, stamp);
   ```

2. **GREEN** Create `Timestamped<T>(T Value, Timestamp At)`. Use it in
   factory method signatures and inside `UserAccount.Register` to defer
   timestamp selection to the application service.

3. **REFACTOR** Lift `Timestamp.UtcNow(clock)` call out of the domain and
   pass `Timestamped` into the aggregate.

---

## Task 8 — `PersonName`, `Email`, `PhoneNumber` smart constructors to bind over errors
**Goal:** Remove `Seq1(error)` wrappers by turning single-arg `Either<DomainError,T>`
   into `Either<Seq<DomainError>, T>` natively.

1. **RED** `ValueObjectTests.PersonNameCreate_MapsSingleDomainErrorToSingleItemSeq`.

2. **GREEN** Introduce a tiny `Either<DomainError,T> LiftToSeq(Either<DomainError,T> e)`:
   ```csharp
   private static Either<Seq<DomainError>, T> Lift<T>(Either<DomainError, T> e) =>
       e.MapLeft(err => Seq1(err));
   ```
   Or simpler: change VO factories to return
   `Either<Seq<DomainError>, Self>` directly.

3. **REFACTOR** User the unified return shape everywhere.

---

## Task 9 — Domain methods return either `Either<Seq<DomainError>, Unit>` or `Either<Seq<DomainError>, UserAccount>`
**Goal:** All domain methods are monadically chainable — no method body
   contains `if` or `switch`.

1. **RED** `UserAccountTests.Rename_ReturnsEitherOfSequentialShape`.

2. **GREEN** Rewrite `Rename`, `ChangeEmail`, `ChangePhoneNumber` to compose:
   ```csharp
   public Either<Seq<DomainError>, UserAccount> Rename(PersonName next, TimeProvider clock) =>
       Name.Equals(next)
           ? Right<Seq<DomainError>, UserAccount>(this)
           : NextSnapshot(...).WithNewName(next).Raise(UserAccountNameChangedDomainEvent.New);
   ```

3. **REFACTOR** Use `with` expressions (C# 10+) on a record-style aggregate
   or a manual `With(...)` builder. No internal setters remaining.

---

## Task 10 — `IUserAccountLookup.EmailExistsAsync(Email email)`
Already exists. Confirm tests. No code change.

---

## Task 11 — `RegisterUserService` and `RenameUserService` are pure monads
**Goal:** Application services compose VOs → aggregate → ports with LINQ
   query only.

1. **RED** `RegisterUserServiceTests.RegisterAsync_PureMonadComposesAllSteps`
   expects the test to instantiate the service with a fake clock and verify
   that no method body has any `if`.

2. **GREEN** Implement `RegisterAsync` via `from ... in ... select`.

3. **REFACTOR** Extract `ToDto(UserAccount)` mapping helper class
   `UserAccountMapping.ToDto` — eliminates duplication between service and
   query-side.

---

## Task 12 — DTOs use `string?` instead of `Option<string>`
**Goal:** Eliminate the `Option` leak across the application/presentation
   boundary.

1. **RED** `RegisteredUserDtoTests.PhoneNumber_IsStringNullable` — verify
   the contract.

2. **GREEN** Change `RegisteredUserDto.PhoneNumber` and
   `UserAccountQueryDto.PhoneNumber` to `string?`. Map at construction:
   ```csharp
   account.PhoneNumber.Map(p => p.Value).Untyped()  // Option<string> -> string?
   ```

3. **REFACTOR** Remove the `foreach { return value; return null; }` pattern
   everywhere; use `Match`.

---

## Task 13 — `DomainErrorHttpMap` table + `ProblemResult` cleanup
**Goal:** Presentation layer reads category from a static table, never
   pattern-matches a concrete domain type.

1. **RED** `ProblemResultTests.FromErrors_GroupUnderOneCategory` and
   `FromErrors_PicksConflictIfAnyConflictPresent`.

2. **GREEN**
   - Build `DomainErrorHttpMap.Table` table.
   - Rewrite `ProblemResult.FromErrors` to:
     ```csharp
     var dominant = errors.Fold(...) ;  // Seq.groupBy + Seq.head — pure, no if
     var (status, title) = DomainErrorHttpMap.Resolve(dominant);
     ```
   - Remove `is ... or ...` from `ProblemResult`.

3. **REFACTOR** Add `DomainErrorCategory.Any` for the multi-category case.

---

## Task 14 — `global using static LanguageExt.Prelude` and `global using LanguageExt` in csproj
**Goal:** Reduce boilerplate at every file.

1. **RED** No test needed (cosmetic). Skip.

2. **GREEN** Add
   ```xml
   <ItemGroup>
     <Using Include="LanguageExt" />
     <Using Include="static LanguageExt.Prelude" />
   </ItemGroup>
   ```

3. **REFACTOR** Drop `using LanguageExt;` and `using static LanguageExt.Prelude;`
   lines from every file in the tutorial project.

---

## Task 15 — Test refactor (or split) as needed
**Goal:** Continue enforcing architectural rules through tests after refactor:
- `DomainLayer_DoesNotReferenceInfrastructureOrEntityFramework`
- `DomainLayer_DoesNotExposePhoneNumberValue`
- `DomainLayer_DoesNotExposeCreatedByOrLastModifiedBy`
- `ApplicationLayer_DoesNotReferenceInfrastructure`
- `DomainLayer_HasZeroIfAndSwitchStatements` *(analyzer via Roslyn or
  brute-force `Regex.IsMatch` reflection over IL — used as smoke test)*

1. **RED** for any missing rule test.

2. **GREEN** Implement any missing rule.

3. **REFACTOR** Group rule tests in `ArchitectureTests.cs`.

---

## Task 16 — Update README + run final verification
**Goal:** Documentation reflects new design; manual checklist of:
   - All files compile (visual review)
   - Tests cover each new method
   - No `if`/`switch` left in Domain or Application via grep
   - Architecture rules pass

1. **RED** none.
2. **GREEN** Update `README.md` + write a short `BEFORE → AFTER` table.
3. **REFACTOR** Final pass.

---

## Definition of Done

- All `dotnet test` green.
- `DomainLayer_DoesNotReferenceInfrastructureOrEntityFramework` and
  `DomainLayer_HasZeroIfAndSwitchStatements` pass.
- No naked `if`/`switch` left in `01-Domain/**/*.cs` and
  `02-Application/**/*.cs` (grep `\bif\s*\(` and `\bswitch\s*[\(\{]`).
- No `DateTime.UtcNow` outside the `Timestamp.UtcNow` factory and the test
  assembly (`grep -R` over production code).
- No `Document` reference, no `NormalizeDocument` reference, no
  `PhoneNumberValue` reference.
- README documents how decisions were taken.
