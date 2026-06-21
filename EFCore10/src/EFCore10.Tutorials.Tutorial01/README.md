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
- `Experimento N`: ação executada.
- `Código observado`: trecho curto de C# que demonstra a ação do experimento.
- `Evidências`: valores medidos em runtime, como IDs, URLs e contagens.
- `Conclusão`: como interpretar o resultado.
- `Limpeza`: remoção dos dados criados pela demonstração.

Os snippets mostrados no console são curados para focar no conceito do
experimento; eles não tentam reproduzir o arquivo inteiro. As evidências
mostram o que foi verificado depois de cada operação, em vez de depender apenas
da ausência de erro.

## Experimentos

1. Criar e consultar um blog, confirmando o `BlogId` gerado e a URL recuperada.
2. Atualizar o blog e adicionar um post, confirmando o `PostId`, o `BlogId` do post e a contagem persistida.
3. Remover o blog, confirmando por consulta que não restaram registros com o ID removido.

## Regra prática

Este é o caminho mais simples para começar com EF Core, mas ele ainda deixa a
configuração dentro do `DbContext`. O Tutorial02 melhora esse desenho movendo a
configuração para `appsettings.json`, DI e Fluent API.
