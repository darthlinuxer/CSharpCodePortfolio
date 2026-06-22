# Tutorial 06 - DDD + EF Core 10 + SQLite

Este tutorial modela um blog com domínio rico e persistência SQLite.

O domínio usa value objects para validar entrada, `Person<TId>` como abstração OO,
`User` concreto para cadastro/login, papéis de blog (`BlogOwner` e `Author`) em tabelas explícitas,
estados comportamentais sem `enum`, e domain events persistidos em uma tabela de outbox.

## Pontos principais

- `PersonName`, `Cpf`, `Email`, `PhoneNumber`, `ZipCode`, `StateCode`, `Address`, `Contact`, `UserName`, `PasswordHash`, `BlogName`, `BlogUrl`, `PostTitle` e `PostContent` validam e normalizam dados.
- Normalizacao mecanica de strings fica em extensions internas neutras; regras de negocio continuam nos value objects.
- `PasswordHash` gera e verifica hashes Argon2id com salt por senha, parametros versionados e comparacao em tempo constante.
- IDs fortes usam `Guid` como tipo .NET e `Guid.CreateVersion7()` para gerar UUIDv7.
- Entidades e complex types mantem construtores para EF e usam `null!` pontual nas propriedades que o EF materializa; factories e mapeamentos `IsRequired` preservam invariantes no uso normal.
- `Person<TId>` modela dados e comportamentos comuns de pessoa sem virar tabela; `User` é persistido em `Users`.
- `BlogOwner` guarda histórico de ownership por blog, com uma linha ativa por vez; `Author` modela convite aceito/revogado para postar.
- `Blog` protege regras de dono atual, transferência, convite de autores e autorização de postagem.
- `Post` muda estado por comportamento: `Draft -> Published -> Archived`.
- Estados persistem chaves estáveis (`UserState`, `BlogState`, `AuthorState`, `PostState`) e são reconstruídos por registries, sem `int` e sem `switch`.
- `DomainEvents` são capturados em `OutboxMessages`; a demo salva, exibe e limpa os eventos em memória.
- O domínio fica separado por subdomínio: `Models/Common`, `Models/People` e `Models/Blogging`, com subfolders para entities, value objects, states e events.
- `BloggingContext` expõe apenas aggregates concretos (`Users`, `Blogs`, `Posts`); roles (`BlogOwners`, `Authors`) continuam persistidos, mas não são `DbSet` públicos.

## Execução

```bash
dotnet run --project src/EFCore10.App -- run 06
```

O runtime recria o schema SQLite, registra usuários, cria um blog, convida e aceita um autor,
publica e arquiva um post, transfere ownership, salva no SQLite e consulta tudo de volta com `Include`.
