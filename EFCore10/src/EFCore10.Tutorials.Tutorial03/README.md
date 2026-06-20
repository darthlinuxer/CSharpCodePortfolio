# Tutorial03 - Pooled DbContext Factory

Este tutorial demonstra o uso de `AddPooledDbContextFactory<TContext>` com
SQLite. O foco é entender o ciclo de vida do `DbContext` criado por uma factory
pooled.

## O que está sendo demonstrado

`IDbContextFactory<TContext>` cria contextos sob demanda. Quando usamos
`AddPooledDbContextFactory<TContext>`, o EF Core pode reutilizar uma instância de
`DbContext` depois que ela é descartada.

O tutorial configura o pool com tamanho `1` para tornar esse comportamento
visível. Ele cria um contexto, executa uma consulta, descarta a instância e
cria outro contexto. Quando o hash da instância se repete, isso indica que o
contexto voltou ao pool e foi reutilizado.

## Onde falha

Um contexto criado pela factory não é gerenciado por um escopo de DI. O código
que chama `CreateDbContextAsync` também é responsável por chamar
`DisposeAsync`.

Quando um contexto não é descartado, ele continua fora do pool. Nesse caso, a
próxima chamada da factory precisa criar outra instância. O tutorial imprime:

```text
Failure: context was not returned to the pool because it was not disposed
```

## Solução

Trate todo contexto criado pela factory como um recurso de vida curta:

```csharp
await using var context = await factory.CreateDbContextAsync(cancellationToken);
```

Esse padrão garante que o contexto seja descartado no fim da operação e possa
voltar ao pool.

## O que fica para os próximos tutoriais

Este tutorial não cobre estado dinâmico nem connection pooling. Esses assuntos
ficaram separados para reduzir a mistura de conceitos:

- Tutorial04: estado dinâmico em contexto pooled.
- Tutorial05: diferença entre DbContext pooling e connection pooling.

Referência: https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#dbcontext-pooling
