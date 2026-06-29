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

Estrutura física das camadas:

- `01-Domain`: núcleo do modelo; não conhece EF Core, HTTP ou application service;
- `02-Application`: casos de uso, DTOs e portas;
- `03-Infrastructure`: adapter externo driven/outbound para EF Core;
- `03-Presentation`: adapter externo driving/inbound para HTTP.

`Infrastructure` e `Presentation` usam o mesmo número porque são adapters no
mesmo anel externo. A numeração ensina dependência arquitetural, não ordem de
execução. Os namespaces continuam sem número: `...Domain`, `...Application`,
`...Infrastructure` e `...Presentation`.

Estrutura interna do domínio:

- `01-Domain/Aggregates/UserAccounts`: aggregate root, eventos e erros do `UserAccount`;
- `01-Domain/Entities`: base entity e contratos comuns;
- `01-Domain/ValueObjects`: `PersonName`, `Email`, `PhoneNumber`, `Timestamp`;
- `01-Domain/Events`: contratos comuns de domain events;
- `01-Domain/Errors`: tipos base `DomainError` e `DomainErrorCode`.

Estrutura interna das outras camadas:

- `02-Application/Commands`: DTOs e services de caso de uso;
- `02-Application/Queries`: DTOs e portas de leitura;
- `02-Application/Persistence`: writer fino e unit of work;
- `03-Infrastructure/Persistence`: `DbContext`, mappings, writer EF e commit;
- `03-Infrastructure/Queries`: implementações EF das queries;
- `03-Presentation/Http`: tradução de `Either` para HTTP e `ProblemDetails`.

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
public Email Email { get; }
```

> **Document foi removido** (Task 4 do plano de refatoração monádico). Modelagem de PF (CPF) vs PJ (CNPJ) é tratada em tutorial dedicado futuro — não é responsabilidade do agregado de cadastro abstrato. O campo `Document` deixou a API do domínio junto com `NormalizeDocument` e os erros relacionados.

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
criado, ele levanta `UserAccountRegisteredDomainEvent`. Mudanças posteriores chamam
métodos de domínio como `Rename`, `ChangeEmail` e `ChangePhoneNumber`, que
levantam `UserAccountNameChangedDomainEvent`, `UserAccountEmailChangedDomainEvent`
e `UserAccountPhoneNumberChangedDomainEvent`. A infraestrutura persiste a linha
e limpa os eventos depois de `SaveChangesAsync`.

`DomainError` é o tipo base e cada falha esperada é um tipo concreto próximo do
dono da regra. `EmailInvalidError` fica perto de `Email`; `UserAccountDocumentDuplicateError`
fica perto de `UserAccount`. Isso evita o acoplamento errado em que value objects
conhecem a camada `Application` e também evita um catálogo global baseado em
strings. `DomainErrorCode` continua existindo para serializar o erro em HTTP e
evidência de console; regra interna compara tipos concretos.
`DomainException` não é usada neste tutorial: regra esperada retorna
`Either<DomainError,T>`. Exception fica para falha técnica ou estado impossível
fora do fluxo normal.

## 4. Persistência com EF Core 10

O tutorial persiste `UserAccount` diretamente. Não existe mais `UserRecord`.
`RegistrationDbContext` expõe o `DbSet` e implementa a unit of work:

```csharp
public DbSet<UserAccount> Users => Set<UserAccount>();
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
```

O mapping fica em `03-Infrastructure/Persistence/ConfigurationMappings/UserAccountConfiguration`
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

Duplicidade de email compara o value object mapeado pelo EF no adapter de
infraestrutura:

```csharp
return dbContext.Users
    .AsNoTracking()
    .AnyAsync(user => user.Email.Value == email.Value, cancellationToken);
```

O aggregate não mantém scalar auxiliar de lookup. O tutorial usa SQLite em
memória porque é um provider relacional leve e traduz o acesso ao value object
mapeado por `ComplexProperty`. A coluna `Email` continua configurada no mapping
e pode receber índice/constraint.

Não há repository genérico. O writer é propositalmente fino:

```csharp
public interface IUserAccountWriter
{
    void Add(UserAccount account);
    void Delete(UserAccount account);
}
```

O commit fica no `IRegistrationUnitOfWork`. Updates são rastreados pelo próprio
EF Core.

## 5. Fluxo de cadastro

`RegisterUserService.RegisterAsync` executa:

1. recebe `RegisterUserRequest`;
2. chama `UserAccount.Create(...)`;
3. se o domínio retornar `Left`, devolve `Left(Seq<DomainError>)`;
4. se o aggregate for válido, consulta fatos de duplicidade via `IUserAccountLookup`;
5. chama `UserAccount.EnsureCanBeRegistered(...)` para o domínio decidir a regra;
6. adiciona via `IUserAccountWriter`;
7. commita via `IRegistrationUnitOfWork`;
8. devolve `Right(RegisteredUserDto)`.

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
Falhas esperadas viram `Results.Problem(...)` com extensão `errors`. O status
`409 Conflict` é escolhido por pattern matching de tipo concreto, não por
comparação de string.

## 7. O que ficou fora

`Validation` não entra neste tutorial. `Seq<DomainError>` cobre a lista de
erros sem aumentar a superfície conceitual. Use `Validation` quando quiser
acumulação applicative como assunto próprio.
