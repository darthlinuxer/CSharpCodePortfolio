# Tutorial05 - DbContext pooling vs connection pooling

Este tutorial explica a diferença entre `DbContext` pooling e connection
pooling.

Os dois conceitos podem existir ao mesmo tempo, mas resolvem custos diferentes.
Confundir os dois leva a expectativas erradas sobre o que o EF Core está
reutilizando.

A saída do console usa painéis e separadores para mostrar `Contexto`,
`Pergunta central`, `Hipótese`, `Preparação`, `Experimento N`, `Observação`,
`Conclusão` e `Limpeza`. Isso separa visualmente o objeto `DbContext` da
conexão ADO.NET.

## DbContext pooling

`DbContext` pooling é gerenciado pelo EF Core. Quando usamos
`AddPooledDbContextFactory<TContext>`, o EF Core pode reutilizar uma instância
de `DbContext` depois que ela é descartada.

O tutorial configura o pool com tamanho `1` e imprime o hash da instância. Se o
mesmo hash aparecer em duas operações, a mesma instância de `DbContext` foi
reutilizada pelo pool.

Isso reduz parte do custo de criar e inicializar objetos internos do contexto.

## Connection pooling

Connection pooling é gerenciado pelo provedor ADO.NET do banco de dados, não
pelo `DbContext`. Ele trata do reaproveitamento de conexões físicas ou lógicas
com o banco.

O tutorial mostra o estado da conexão ADO.NET antes e depois de uma consulta EF.
Mesmo quando o `DbContext` volta ao pool, a conexão continua sendo aberta e
fechada conforme a operação precisa dela.

## Onde a confusão aparece

Um contexto pooled não significa uma conexão permanentemente aberta. O objeto
`DbContext` pode ser reaproveitado enquanto a conexão ADO.NET permanece fechada
entre operações.

A saída esperada inclui:

```text
Observação DbContext em uso: hash <valor>. Estado da conexão ADO.NET antes da query EF: Closed.
Observação A query leu 1 blog(s). Depois da query EF, o estado da conexão ADO.NET é Closed.
Conclusão DbContext pooling reutiliza objetos de contexto; connection pooling é responsabilidade do provedor ADO.NET.
```

## Regra prática

Use `DbContext` pooling para reduzir o custo de criação de contextos em caminhos
de alta frequência. Trate connection pooling como uma responsabilidade do
provedor ADO.NET e da string de conexão.

Não use `DbContext` pooling como estratégia para manter conexões abertas.

Referência: https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#connection-pooling-considerations
