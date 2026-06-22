# Tutorial 06 - DDD + EF Core 10 + SQLite

Este tutorial modela um blog com domínio rico e persistência SQLite.

O domínio usa value objects para validar entrada, `Person<TId>` como abstração OO,
`User` concreto para cadastro/login, `BlogMembership` como papel contextual do usuário no blog,
estados comportamentais sem `enum`, `Timestamp` como VO temporal UTC e domain events traduzidos
para uma tabela de outbox processável.

## Pontos principais

- `PersonName`, `Cpf`, `Email`, `PhoneNumber`, `ZipCode`, `StateCode`, `Address`, `Contact`, `UserName`, `PasswordHash`, `BlogName`, `BlogUrl`, `PostTitle` e `PostContent` validam e normalizam dados.
- Normalizacao mecanica de strings fica em extensions internas neutras; regras de negocio continuam nos value objects.
- `PasswordHash` gera e verifica hashes Argon2id com salt por senha, parametros versionados e comparacao em tempo constante.
- IDs fortes usam `Guid` como tipo .NET e `Guid.CreateVersion7()` para gerar UUIDv7.
- `Timestamp` encapsula datas UTC e é usado em memberships, posts, eventos e outbox.
- Entidades e complex types mantem construtores para EF e usam `null!` pontual nas propriedades que o EF materializa; factories e mapeamentos `IsRequired` preservam invariantes no uso normal.
- `Person<TId>` modela dados e comportamentos comuns de pessoa sem virar tabela; `User` é persistido em `Users`.
- `BlogMembership` unifica owner e author em `BlogMemberships`, com role (`Owner`/`Author`), state (`Pending`/`Active`/`Revoked`/`Ended`) e histórico temporal.
- `Blog` protege regras de dono atual, transferência, convite de autores e autorização de postagem.
- `Post` muda estado por comportamento: `Draft -> Published -> Archived`, registrando `CreatedOnUtc`, `PublishedOnUtc` e `ArchivedOnUtc` para queries por range.
- Estados persistem chaves estáveis (`UserState`, `BlogState`, `MembershipRole`, `MembershipState`, `PostState`) e são reconstruídos por registries, sem `int` e sem `switch`.
- `DomainEvents` carregam fatos do domínio; o mapper do outbox traduz esses fatos para `OutboxMessages` com `EventName`, `EventVersion`, aggregate, status, retry metadata e payload JSON estável.
- Sem worker nesta etapa, mensagens da outbox permanecem `Pending`; os estados do workflow mudam nos aggregates antes da persistência.
- O domínio fica separado por subdomínio: `Models/Common`, `Models/People` e `Models/Blogging`, com subfolders para entities, value objects, states e events.
- `BloggingContext` expõe apenas aggregates concretos (`Users`, `Blogs`, `Posts`); memberships continuam persistidas, mas não são `DbSet` público.

## Execução

```bash
dotnet run --project src/EFCore10.App -- run 06
```

O runtime recria o schema SQLite, registra usuários, cria um blog, convida e aceita um autor,
publica e arquiva um post, transfere ownership, salva no SQLite, consulta posts publicados por range
de data e mostra eventos no outbox com envelope processável.
