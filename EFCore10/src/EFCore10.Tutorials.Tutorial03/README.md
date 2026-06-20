# Tutorial03 - Pooled DbContext Factory

Este tutorial demonstra `AddPooledDbContextFactory<TContext>` com SQLite e um
modelo simples de blogs e posts.

## O que muda

`AddDbContext<TContext>` registra o contexto para ser resolvido diretamente pelo
container, normalmente com ciclo de vida scoped. Esse e o caminho comum quando
cada operacao precisa de um unico `DbContext` ligado ao escopo atual.

`AddDbContextPool<TContext>` mantem o mesmo formato de injecao direta, mas
permite que o EF Core reutilize instancias de `DbContext` depois que elas sao
descartadas. Isso reduz alocacoes e o custo repetido de configurar servicos
internos do contexto.

`AddPooledDbContextFactory<TContext>` combina pooling com factory. Em vez de
receber o `DbContext` diretamente, o codigo recebe `IDbContextFactory<TContext>`
e cria contextos sob demanda. Cada contexto criado pela factory deve ser
descartado explicitamente; ao ser descartado, ele volta para o pool.

## Por que usar factory pooled

Factory e util quando o codigo precisa controlar o ciclo de vida do contexto,
criar mais de um contexto dentro do mesmo escopo ou executar uma unidade de
trabalho curta sem prender o `DbContext` ao ciclo de vida do servico que foi
injetado.

O pooling e especialmente interessante em caminhos de alta frequencia, onde o
custo de criar e inicializar muitos contextos pequenos aparece no perfil de
performance. Com pooling, o custo de setup tende a ser pago uma vez e a mesma
instancia pode ser limpa e reutilizada em operacoes posteriores.

## DbContext pooling nao e connection pooling

O pool de `DbContext` e gerenciado pelo EF Core e reutiliza objetos de contexto.
O pool de conexoes e gerenciado pelo provider ADO.NET do banco de dados e
reutiliza conexoes fisicas/logicas com o banco. Eles resolvem custos diferentes
e podem existir ao mesmo tempo.

## Cuidado com estado mutavel

Um contexto pooled pode ser reutilizado por varias operacoes depois de ser
descartado. Na pratica, trate a instancia como reutilizavel pelo container e nao
guarde nela estado que muda por request, por tenant ou por usuario.

Tambem nao use `OnConfiguring` para configurar estado variavel. Em contextos
pooled, `OnConfiguring` roda quando a instancia e criada pela primeira vez, nao
a cada uso. Se uma operacao precisa de estado dinamico, injete esse estado fora
do contexto ou crie uma factory scoped que configure o contexto antes de
entrega-lo ao codigo chamador.

Referencia: https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant#dbcontext-pooling
