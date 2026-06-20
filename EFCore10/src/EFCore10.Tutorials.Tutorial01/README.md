# Tutorial01 - Modelagem simples com EF Core

Este tutorial apresenta o fluxo mínimo para usar EF Core com SQLite: criar
modelos, criar um `DbContext`, gerar uma migration e executar operações CRUD.

## O que está sendo demonstrado

O exemplo usa os modelos `Blog` e `Post`, com o provider SQLite configurado em
`OnConfiguring`. A migration `InitialCreate` representa o schema usado pelo
tutorial.

A saída do console usa a mesma estrutura visual dos demais tutoriais:

- `Contexto`: provider, tipo de configuração e migration usada.
- `Pergunta central`: qual dúvida o tutorial responde.
- `Hipótese`: qual comportamento esperamos observar.
- `Preparação`: passos necessários antes do CRUD.
- `Experimento N`: ação executada, observações e conclusão.
- `Limpeza`: remoção dos dados criados pela demonstração.

## Experimentos

1. Criar e consultar um blog.
2. Atualizar o blog e adicionar um post.
3. Remover o blog.

## Regra prática

Este é o caminho mais simples para começar com EF Core, mas ele ainda deixa a
configuração dentro do `DbContext`. O Tutorial02 melhora esse desenho movendo a
configuração para `appsettings.json`, DI e Fluent API.
