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
- `Experimento N`: ação executada.
- `Código observado`: trecho curto de C# que demonstra a decisão técnica.
- `Evidências`: valores medidos em runtime, como provider, banco, IDs e contagens.
- `Conclusão`: como interpretar o resultado.
- `Limpeza`: remoção dos dados criados pela demonstração.

Os snippets mostrados no console são curados para destacar DI, `appsettings` e
Fluent API sem despejar arquivos inteiros no terminal. As evidências conectam
esses snippets ao comportamento observado: qual `DbContext` foi injetado, qual
provider foi usado e quais registros foram persistidos ou removidos.

## Experimentos

1. Inserir um blog usando o contexto injetado e confirmar o `BlogId` gerado.
2. Consultar o blog com `Include` e confirmar a coleção carregada.
3. Atualizar o blog e adicionar um post, confirmando o relacionamento 1:N no banco.
4. Remover o blog e confirmar por consulta que os registros não permanecem.

## Regra prática

Use DI para controlar a criação do `DbContext` e use Fluent API quando quiser
deixar a configuração do modelo explícita, revisável e separada das classes de
entidade.
