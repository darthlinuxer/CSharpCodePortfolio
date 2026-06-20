# Tutorial04 - Estado dinâmico em contextos pooled

Este tutorial mostra um cuidado específico ao usar `DbContext` com pooling: estado dinâmico não deve ser configurado em `OnConfiguring`.

O exemplo usa um cenário multi-tenant simples. Cada `Blog` possui um `TenantId`, e o `DbContext` correto usa um filtro global para mostrar apenas os dados do tenant atual.

## O problema

Em contextos pooled, a mesma instância de `DbContext` pode ser reutilizada em várias operações. Por isso, o contexto se comporta de forma parecida com um
objeto reutilizável pelo contêiner.

O método `OnConfiguring` não roda a cada uso da instância. Ele roda quando a instância é inicializada. Se o código tenta carregar estado variável ali, como
o tenant atual, esse estado pode ficar antigo quando a instância voltar do pool.

## Onde falha

O tutorial inclui um contexto ruim chamado `BadOnConfiguringTenantContext`. Ele lê o tenant atual em `OnConfiguring` a partir de um estado ambiente.

O fluxo executado é:

1. O estado ambiente começa como `tenant-a`.
2. O contexto é criado e passa a enxergar dados do `tenant-a`.
3. O contexto é descartado e volta ao pool.
4. O estado ambiente muda para `tenant-b`.
5. A factory entrega a mesma instância reaproveitada.

Como `OnConfiguring` não roda novamente para atualizar o estado, o contexto continua configurado com `tenant-a`. A saída mostra:

```text
Failure: OnConfiguring kept tenant-a while ambient tenant changed to tenant-b
```

## Solução

A solução demonstrada é encapsular a factory pooled em uma factory específica do domínio: `TenantAwareBloggingContextFactory`.

Essa factory cria um lease de contexto. Ao criar o lease, ela chama `SetTenant(tenantId)`. Ao descartar o lease, ela chama `ClearTenant()` e depois
descarta o `DbContext`, devolvendo a instância limpa para o pool.

O uso correto fica assim:

```csharp
await using var lease = await tenantAwareFactory.CreateDbContextAsync(
    "tenant-b",
    cancellationToken);

var blogs = await lease.Context.Blogs.ToListAsync(cancellationToken);
```

O tutorial confirma a solução com a mensagem:

```text
Solution: tenant-aware factory returned tenant-b data
```

## Regra prática

Use `OnConfiguring` para configuração estável do contexto. Não use `OnConfiguring` para estado que muda por requisição, usuário ou tenant quando o
contexto usa pooling.

Referência: https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#managing-state-in-pooled-contexts
