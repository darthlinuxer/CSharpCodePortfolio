# Tutorial 30 - LanguageExt.Core pragmático

Este tutorial mostra um cadastro de usuário modelado com DDD tático e
`LanguageExt.Core`: `Option<T>` para ausência válida, `Either<Seq<DomainError>,
T>` para falhas esperadas e EF Core como adapter de infraestrutura.

O projeto alvo é `net10.0` com `LangVersion` fixado em `14.0`. A versão usada
do pacote funcional é `LanguageExt.Core` `4.4.9`.

## Estrutura

- `01-Domain`: aggregate, value objects, eventos, erros e building blocks comuns.
- `02-Application`: caso de uso, DTOs e portas finas.
- `03-Infrastructure`: EF Core, mapping, writer e unit of work.
- `03-Presentation`: tradução HTTP de `Either` para `IResult`.

O domínio preserva ownership por pasta:

- `01-Domain/Common/...`: `AbstractEntity<TId>`, `DomainError`, eventos comuns,
  helpers pequenos e `Timestamp`.
- `01-Domain/Aggregates/UserAccounts/...`: `UserAccount`, erros/eventos do
  aggregate e value objects específicos (`PersonName`, `Email`, `PhoneNumber`).

## Modelo de domínio

`UserAccount` é o aggregate root. Ele mantém `Name` e `Email` como value objects
obrigatórios e `PhoneNumber` como `Option<PhoneNumber>`.

```csharp
public static Either<Seq<DomainError>, UserAccount> Create(
    string? name,
    string? email,
    string? phoneNumber,
    TimeProvider clock)
```

Falhas de regra esperada não usam exception. Cada value object retorna
`Either<Seq<DomainError>, T>` e o aggregate acumula os erros de entrada. O relógio
entra por `TimeProvider`, então timestamps de criação, alteração e eventos são
determinísticos em teste.

`AbstractEntity<TId>` contém somente identidade, `CreatedAt`, `LastModified` e
domain events. Auditoria de ator fica fora do aggregate, na borda de aplicação ou
autenticação quando existir.

## Erros

`DomainError` expõe:

- `Code`: código estável para serialização.
- `Message`: mensagem humana.
- `Category`: taxonomia de domínio (`validation` ou `conflict`).

A apresentação traduz `Category` para HTTP em `DomainErrorHttpMap`. Assim um novo
erro de conflito não obriga o endpoint a conhecer o tipo concreto.

## Persistência

`RegistrationDbContext` persiste o aggregate diretamente. Não há repository
genérico. O writer é fino:

```csharp
public interface IUserAccountWriter
{
    void Add(UserAccount account);
    void Delete(UserAccount account);
}
```

`IRegistrationUnitOfWork.CommitAsync` concentra o commit e converte conflitos
esperados de índice único em `Left(Seq<DomainError>)`. O índice único durável é o
email; o pre-check no application service melhora a resposta, mas a constraint do
banco continua sendo a proteção contra corrida.

`Option<PhoneNumber>` é a API pública do aggregate. EF Core mapeia um backing
value nullable (`PhoneNumberValue`) por `ValueConverter`, mantendo `PhoneNumber?`
fora do contrato público do domínio.

## Fluxo

`RegisterUserService.RegisterAsync` executa:

1. cria o aggregate com `UserAccount.Create(...)`;
2. retorna `Left` se a validação do domínio falhar;
3. consulta `IUserAccountLookup.EmailExistsAsync`;
4. deixa o aggregate decidir `EnsureCanBeRegistered(emailExists)`;
5. adiciona pelo writer;
6. commita pela unit of work;
7. retorna `RegisteredUserDto` com valores primitivos.

`RegistrationEndpoint` fecha o `Either`:

- `Right(dto)` vira `201 Created`;
- `validation` vira `400 BadRequest`;
- `conflict` vira `409 Conflict`.

`Option<string>` só vira `string?` na resposta HTTP.

## Executar

```bash
dotnet test tests/CSharpCodePortfolio.Tutorials.Tutorial30.Tests/CSharpCodePortfolio.Tutorials.Tutorial30.Tests.csproj
dotnet run --project src/CSharpCodePortfolio.App -- run 30
```

O tutorial de console imprime evidências de validação, persistência, conflito por
email e tradução HTTP.
