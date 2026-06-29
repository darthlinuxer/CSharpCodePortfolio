# Tutorial 30 — LanguageExt.Core pragmático

Este tutorial mostra como usar `LanguageExt.Core` em um projeto .NET moderno
para reduzir nullables no domínio, null checks espalhados, `if/else`
procedural, exceptions para regra de negócio esperada e services difíceis de
compor.

## Versão usada

O tutorial usa `LanguageExt.Core` `4.4.9`, versão estável mais recente
confirmada no feed NuGet durante a implementação.

Existe prerelease `5.0.0-beta-77`. Ela não é usada aqui porque este tutorial
quer uma base estável para `Option`, `Either`, `Fin`, `Try`, `Seq`, `Map`,
`Bind`, `Match`, `Some`, `None`, `Right`, `Left` e LINQ query syntax.

`global using LanguageExt;` e `global using static LanguageExt.Prelude;`
são promovidos no csproj, então `Some`, `None`, `Right`, `Left`, `Match`,
`Map`, `Bind`, `Seq`, `Seq1`, etc. estão disponíveis sem `using` explícito
em cada arquivo.

## Arquitetura

Estrutura física das camadas:

- `01-Domain`: núcleo do modelo; não conhece EF Core, HTTP ou application
  service. **Zero `if` / `switch`** — toda a lógica flui via composição
  monádica.
- `02-Application`: casos de uso, DTOs e portas. **Zero `if` / `switch`**.
- `03-Infrastructure`: adapter externo driven/outbound para EF Core.
- `03-Presentation`: adapter externo driving/inbound para HTTP.

`Infrastructure` e `Presentation` usam o mesmo número porque são adapters no
mesmo anel externo. Os namespaces continuam sem número: `...Domain`,
`...Application`, `...Infrastructure` e `...Presentation`.

Domain contém:

- `PersonName`, `Email`, `PhoneNumber`, `Timestamp` como
  `readonly record struct`.
- `Timestamped<T>` para um valor acompanhado do seu instante UTC.
- `DomainError`, `DomainErrorCode`, `DomainErrorCategory`.

Estrutura interna do domínio:

- `01-Domain/Aggregates/UserAccounts`: aggregate root, eventos e erros.
- `01-Domain/Entities`: base `AbstractEntity<TId>` e `IEntity<TId>`.
- `01-Domain/ValueObjects`: `PersonName`, `Email`, `PhoneNumber`, `Timestamp`, `Timestamped<T>`.
- `01-Domain/Events`: contratos comuns de domain events.
- `01-Domain/Errors`: tipos `DomainError`, `DomainErrorCode`,
  `DomainErrorCategory`.

## Conceitos centrais

`Option<T>` representa ausência válida. No tutorial, telefone é opcional;
email é obrigatório:

```csharp
public Email Email { get; }
public Option<PhoneNumber> PhoneNumber { get; }
```

`Either<Seq<DomainError>, T>` é o tipo de retorno uniforme. `Left` carrega
uma sequência de erros (acumulação sem short-circuit); `Right` carrega o
valor. LINQ query syntax do LanguageExt (`from x in y`) mantém a
composição puramente declarativa.

`DomainError.Category` classifica o erro no domínio (`Validation` ou
`Conflict`). A camada de presentation (`DomainErrorHttpMap` table) lê
apenas a categoria, nunca pattern-matches contra o tipo concreto do erro.

`Timestamp.UtcNow(TimeProvider clock)` é a **única** porta para o relógio
no domínio; testes injetam `FakeTimeProvider` para determinismo.

## Persistência com EF Core 10

O aggregate `UserAccount` é mapeado diretamente pelo EF Core 10:

- `ComplexProperty` para `Name` e `Email` (VOs obrigatórios).
- `ValueConverter<Option<PhoneNumber>, string?>` para a propriedade
  opcional `PhoneNumber`. Não há `PhoneNumberValue` espelhado.
- `Ignore` para `DomainEvents`, `CreatedAt`, `LastModified`.

`Document` foi **retirado** (Task 4 do plano de refatoração monádica) —
modelagem PF (CPF) vs PJ (CNPJ) é responsabilidade de um tutorial
dedicado futuro, não do agregado de cadastro abstrato.

## Fluxo HTTP

`RegistrationEndpoint.ToHttpResult` mapeia o resultado via `Match`:

- `Right(dto)` → `201 Created`
- `Left(Seq<DomainError>)` → `ProblemResult.FromErrors(...)` que despacha por
  categoria via `DomainErrorHttpMap`.

Presentation nunca faz pattern matching em tipo concreto de erro — adicionar
novos erros é puramente extensivo (OCP).

## Garantias arquiteturais

Os testes guardam a forma do refactor:

- `DomainLayer_HasNoIfOrSwitchStatements` — falha se algum `if` ou `switch`
  novo entrar em `01-Domain`.
- `ApplicationLayer_HasNoIfOrSwitchStatements` — mesma coisa em
  `02-Application`.
- `DomainLayer_DoesNotCallDateTimeUtcNowDirectly` — apenas
  `Timestamp.UtcNow` lê o relógio.
- `DomainValueObjects_AreStructs` — VOs são readonly record struct.
- `DomainLayer_DoesNotExposePhoneNumberValueMirror` — confirma que o
  mirror de EF foi extinto.
- `Presentation_DoesNotReferenceConcreteDomainErrorTypes` — confirma
  que Presentation despacha por Category, não por tipo concreto.
- `UserAccount_DoesNotExposeDocumentInDomainOrDatabase` — confirma
  retirada do Document.

## O que ficou fora

- `Validation` acumulação applicative como assunto próprio.
- Modelagem PF/PJ (CPF/CNPJ).
- Auditoria de ator (`CreatedBy`/`LastModifiedBy`); pertence ao seam de
  autenticação, não à entity base.
