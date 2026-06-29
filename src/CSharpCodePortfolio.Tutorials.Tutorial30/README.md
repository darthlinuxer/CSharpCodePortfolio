# Tutorial 30 - LanguageExt.Core pragmático

Este tutorial mostra como usar `LanguageExt.Core` em um projeto .NET moderno
para reduzir nullables no domínio, null checks espalhados, `if/else`
procedural, exceptions para regra de negócio esperada e services difíceis de
compor.

## Versão usada

O tutorial usa `LanguageExt.Core` `4.4.9`, versão estável mais recente
confirmada no feed NuGet durante a implementação.

Existe prerelease `5.0.0-beta-77`. Ela não é usada aqui porque este tutorial
quer uma base estável para `Option`, `Either`, `Fin`, `Try`, `Seq`, `Map`,
`Bind`, `Match`, `Some`, `None`, `Right`, `Left` e LINQ query syntax. A v5 beta
pode ser avaliada em projeto novo que aceite mudanças de API.

Usings principais:

```csharp
using LanguageExt;
using static LanguageExt.Prelude;
```

Estrutura do domínio:

- `Domain/Aggregates`: aggregate roots, como `UserAccount`;
- `Domain/Entities`: base entity e contratos comuns;
- `Domain/ValueObjects`: `PersonName`, `Email`, `PhoneNumber`, `Timestamp`;
- `Domain/Events`: domain events;
- `Domain/Errors`: `DomainError` e catálogo `DomainErrors`.

Estrutura das camadas:

- `Application/Commands`: DTOs e services de caso de uso;
- `Application/Queries`: DTOs e portas de leitura;
- `Application/Persistence`: portas finas de escrita;
- `Infrastructure/Persistence`: `DbContext`, mappings e writer EF;
- `Infrastructure/Queries`: implementações EF das queries;
- `Http`: tradução de `Either` para HTTP e `ProblemDetails`.

## 1. O problema do C# tradicional

O arquivo `Traditional/TraditionalNullRegistrationExample.cs` mostra um cadastro
de usuário com `string?`, `Email?`, `PhoneNumber?`, vários `if`, `throw` para
regra esperada, service retornando `null` e controller interpretando estados
implícitos.

O problema não é o nullable em DTO de entrada. O problema é deixar o domínio e
o fluxo de aplicação dependerem de ausência implícita:

```csharp
public string? Name { get; set; }
public string? Document { get; set; }
public Email? Email { get; set; }
public PhoneNumber? PhoneNumber { get; set; }
```

Depois disso, cada camada precisa perguntar de novo:

- nome existe?
- documento existe?
- email ausente é válido ou erro?
- `null` do service significa conflito, não encontrado ou falha?
- exception é bug técnico ou regra de negócio?

## 2. Tipos centrais do LanguageExt.Core

`Option<T>` representa ausência válida. Neste tutorial, telefone é opcional;
email é obrigatório:

```csharp
public Email Email { get; }
public Option<PhoneNumber> PhoneNumber { get; }
```

`Either<L,R>` representa erro esperado. `Left` carrega erro; `Right` carrega
valor válido:

```csharp
Either<DomainError, Email> email = Email.Create("ada@example.com");
```

`Fin<T>` representa sucesso/falha com `LanguageExt.Common.Error`. Use quando a
falha genérica da biblioteca for suficiente; prefira `Either<DomainError,T>`
quando o domínio precisa de códigos próprios.

`Try<T>` captura exceções em bordas técnicas. Em `LanguageExt.Core` `4.4.9`,
executar `Try<T>` retorna `Result<T>`, não `Fin<T>`.

`Seq<T>` é coleção funcional. Aqui o service retorna
`Either<Seq<DomainError>, RegisteredUserDto>` para permitir mais de um
erro esperado.

`Map` transforma valor dentro do contexto. `Bind` encadeia funções que também
retornam contexto. `Match` fecha o fluxo explicitamente. LINQ query syntax
ajuda quando vários `Either` precisam ser compostos.

## 3. Substituindo nullables no domínio

Campo obrigatório fica direto:

```csharp
public PersonName Name { get; }
public string Document { get; }
public Email Email { get; }
```

Campo opcional vira `Option<T>`:

```csharp
public Option<PhoneNumber> PhoneNumber { get; }
```

Assim o aggregate `UserAccount` não tem estado parcialmente inválido. Entrada
bruta ainda pode ter `string?`, mas ela para em `RegisterUserRequest`. Quem
transforma esses valores em modelo válido é `UserAccount.Create(...)`, retornando
`Either<Seq<DomainError>, UserAccount>`.

`UserAccount` herda de `AbstractEntity<Guid>`, então identity, metadados e
domain events ficam no domínio, não no application service. A identity é criada
no construtor do domínio com `Guid.CreateVersion7()`; quando EF Core materializa
do banco, ele popula o valor salvo na propriedade `Id`. Quando o aggregate é
criado, ele levanta `UserRegisteredDomainEvent`. Mudanças posteriores chamam
métodos de domínio como `Rename`, `ChangeEmail` e `ChangePhoneNumber`, que
levantam `UserNameChangedDomainEvent`, `UserEmailChangedDomainEvent` e
`UserPhoneNumberChangedDomainEvent`. A infraestrutura persiste a linha e limpa
os eventos depois de `SaveChangesAsync`.

`DomainError` e `DomainErrors` ficam no namespace `Domain`. Isso evita o
acoplamento errado em que value objects conhecem a camada `Application`.
`DomainException` não é usada neste tutorial: regra esperada retorna
`Either<DomainError,T>`. Exception fica para falha técnica ou estado impossível
fora do fluxo normal.

## 4. Persistência com EF Core 10

O tutorial persiste `UserAccount` diretamente. Não existe mais `UserRecord`.
`RegistrationDbContext` expõe:

```csharp
public DbSet<UserAccount> Users => Set<UserAccount>();
```

O mapping fica em `Infrastructure/Persistence/ConfigurationMappings/UserAccountConfiguration`
e usa `ComplexProperty` para value objects:

```csharp
builder.ComplexProperty(user => user.Name, name =>
{
    name.Property(value => value.Value)
        .HasColumnName("Name")
        .HasMaxLength(200)
        .IsRequired();
});
```

Documento ficou como scalar normalizado no aggregate para evitar uma classe de
persistência e o value object `DocumentNumber` foi removido:

```csharp
public string Document { get; private set; }
```

`Option<T>` continua sendo a API do domínio para telefone. EF Core não mapeia
`Option<T>` diretamente; ele mapeia uma propriedade interna nullable como
complex property opcional do EF Core 10:

```csharp
public Option<PhoneNumber> PhoneNumber => ToOption(PhoneNumberValue);
internal PhoneNumber? PhoneNumberValue { get; private set; }
```

Com o provider InMemory, predicates sobre complex properties podem ficar mais
limitados do que em provider relacional. Por isso o aggregate mantém
`EmailLookupValue`, um estado interno sincronizado por `Create` e `ChangeEmail`,
para permitir `AnyAsync` de duplicidade sem consultar `Users.Local`. O domínio
continua expondo `Email` como value object obrigatório.

## 5. Fluxo de cadastro

`RegisterUserService.RegisterAsync` executa:

1. recebe `RegisterUserRequest`;
2. chama `UserAccount.Create(...)`;
3. se o domínio retornar `Left`, devolve `Left(Seq<DomainError>)`;
4. se o aggregate for válido, consulta duplicidade via `IUserAccountLookup`;
5. persiste via `IUserAccountWriter`;
6. devolve `Right(RegisteredUserDto)`.

Telefone opcional usa `Option<T>` no domínio:

```csharp
public Option<PhoneNumber> PhoneNumber => ToOption(PhoneNumberValue);
```

Sem `PhoneNumber?` público, sem exception para regra esperada.

## 6. HTTP explícito

`RegistrationEndpoint.ToHttpResult` transforma o resultado:

- `Right(dto)` -> `201 Created`
- `Left(Invalid*)` -> `400 BadRequest`
- `Left(Duplicate*)` -> `409 Conflict`

Controller ou endpoint não interpreta `null`. Ele fecha o `Either` com `Match`.
Falhas esperadas viram `Results.Problem(...)` com extensão `errors`.

## 7. O que ficou fora

`Validation` não entra neste tutorial. `Seq<DomainError>` cobre a lista de
erros sem aumentar a superfície conceitual. Use `Validation` quando quiser
acumulação applicative como assunto próprio.
