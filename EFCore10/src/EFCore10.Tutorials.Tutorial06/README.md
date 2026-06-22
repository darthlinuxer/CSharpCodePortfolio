# Tutorial 06 - DDD + EF Core 10 + SQLite

Este tutorial modela um blog com domínio rico e persistência SQLite.

O domínio usa value objects para validar entrada, `Author : User : Person` em TPH,
estados comportamentais sem `enum`, e domain events persistidos em uma tabela de outbox.

## Pontos principais

- `PersonName`, `Cpf`, `Email`, `PhoneNumber`, `ZipCode`, `StateCode`, `Address`, `Contact`, `UserName`, `PasswordHash`, `BlogName`, `BlogUrl`, `PostTitle` e `PostContent` validam e normalizam dados.
- Normalizacao mecanica de strings fica em extensions internas neutras; regras de negocio continuam nos value objects.
- `PasswordHash` gera e verifica hashes Argon2id com salt por senha, parametros versionados e comparacao em tempo constante.
- IDs fortes usam `Guid` como tipo .NET e `Guid.CreateVersion7()` para gerar UUIDv7.
- Entidades e complex types mantem construtores para EF e usam `null!` pontual nas propriedades que o EF materializa; factories e mapeamentos `IsRequired` preservam invariantes no uso normal.
- `Author` herda de `User`, que herda de `Person`; o EF persiste tudo em `People` com discriminator técnico e TPH explícito.
- `Post` muda estado por comportamento: `Draft -> Published -> Archived`.
- Estados persistem chaves estáveis (`UserState`, `PostState`) e são reconstruídos por registries, sem `int` e sem `switch`.
- `DomainEvents` são capturados em `OutboxMessages`; a demo salva, exibe e limpa os eventos em memória.
- O domínio fica separado por subdomínio: `Models/Common`, `Models/People` e `Models/Blogging`, com subfolders para entities, value objects, states e events.
- `BloggingContext` expõe apenas aggregates concretos (`Authors`, `Blogs`, `Posts`); `Person` continua mapeado no modelo EF para TPH.

## Execução

```bash
dotnet run --project src/EFCore10.App -- run 06
```

O runtime recria o schema SQLite, cria um autor, cria um blog, adiciona um post,
publica e arquiva o post, salva no SQLite e consulta tudo de volta com `Include`.
