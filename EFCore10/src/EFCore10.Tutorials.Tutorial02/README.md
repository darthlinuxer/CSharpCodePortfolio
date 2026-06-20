# Tutorial02 - DI, appsettings e Fluent API

Este tutorial melhora o desenho do Tutorial01 movendo responsabilidades para os
pontos mais adequados:

- a connection string vem de `appsettings.json`;
- o `BloggingContext` é criado pelo container de DI;
- o serviço `CRUD` recebe o contexto por injeção de dependência;
- o modelo é configurado por Fluent API com `IEntityTypeConfiguration<T>`.

## O que está sendo demonstrado

O tutorial usa SQLite e cria o schema com `EnsureCreatedAsync`, sem migrations
neste passo. A intenção é focar em configuração, DI e modelagem explícita.

A saída do console usa a mesma estrutura visual dos demais tutoriais:

- `Contexto`: provider, banco, arquivo de configuração e abordagem de modelo.
- `Pergunta central`: qual dúvida o tutorial responde.
- `Hipótese`: qual comportamento esperamos observar.
- `Preparação`: configuração carregada e schema garantido.
- `Experimento N`: ação executada, observações e conclusão.
- `Limpeza`: remoção dos dados criados pela demonstração.

## Experimentos

1. Inserir um blog usando o contexto injetado.
2. Consultar o blog e carregar a coleção de posts.
3. Atualizar o blog e adicionar um post.
4. Remover o blog.

## Regra prática

Use DI para controlar a criação do `DbContext` e use Fluent API quando quiser
deixar a configuração do modelo explícita, revisável e separada das classes de
entidade.
