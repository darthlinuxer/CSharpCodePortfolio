# CSharpCodePortfolio .NET 10 Tutorial Refactor Roadmap

## Objetivo

Transformar o repositório em um workspace .NET 10 guiado por um menu raiz de tutoriais. O menu raiz chama o menu existente do `EFCore10` sem refatorar esse projeto, e cada pasta de topo será migrada como uma entrega separada com feedback do usuário.

## Estado Atual

- [x] Criar `.gitignore` raiz para artefatos gerados e worktrees.
- [x] Criar worktree `refactor/phase-zero-menu` para a fase zero.
- [x] Remover `bin/obj` do tracking.
- [x] Criar `CSharpCodePortfolio.slnx` raiz.
- [x] Criar host raiz `src/CSharpCodePortfolio.App`.
- [x] Criar `src/CSharpCodePortfolio.Shared`.
- [x] Criar `src/CSharpCodePortfolio.Tutorials.Abstractions`.
- [x] Criar launcher externo para `EFCore10`.
- [x] Criar template local `portfolio-tutorial`.
- [x] Criar helper compartilhada para imprimir snippets de código reais no console.
- [x] Aplicar a helper no Tutorial01 como piloto.
- [x] Aprimorar a helper com ranges de código e legendas por trecho.
- [x] Aplicar a helper no Tutorial02 após validação do Tutorial01.
- [x] Receber feedback do usuário para prosseguir após o Tutorial02.
- [x] Aplicar a helper no Tutorial03 após validação do Tutorial02.
- [x] Receber feedback do usuário para prosseguir após o Tutorial03.
- [x] Aplicar a helper no Tutorial04 após validação do Tutorial03.
- [x] Receber feedback do usuário para prosseguir após o Tutorial04.
- [x] Aplicar a helper no Tutorial05 após validação do Tutorial04.
- [x] Receber feedback do usuário para prosseguir após o Tutorial05.
- [x] Aplicar a helper no Tutorial06 após validação do Tutorial05.
- [x] Receber feedback do usuário para prosseguir após o Tutorial06.
- [x] Aplicar a helper no Tutorial07 após validação do Tutorial06.
- [x] Receber feedback do usuário para prosseguir após o Tutorial07.
- [x] Aplicar a helper no Tutorial08 após validação do Tutorial07.
- [x] Receber feedback do usuário para prosseguir após o Tutorial08.
- [x] Aplicar a helper no Tutorial09 após validação do Tutorial08.
- [x] Receber feedback do usuário para prosseguir após o Tutorial09.
- [x] Aplicar a helper no Tutorial10 após validação do Tutorial09.
- [ ] Aguardar feedback do usuário antes de aplicar a helper nos tutoriais restantes.
- [ ] Migrar pastas de topo restantes, uma pasta por vez.

## Arquitetura Alvo

- `CSharpCodePortfolio.App`: CLI raiz com comandos `menu`, `list` e `run <id-or-slug>`.
- `CSharpCodePortfolio.Shared`: UI de console compartilhada e leitor de snippets reais para tutoriais do portfolio.
- `CSharpCodePortfolio.Tutorials.Abstractions`: contrato mínimo de descoberta por atributo e execução.
- `EFCore10`: permanece como workspace independente; o host raiz executa o menu dele via `dotnet run --project EFCore10/src/EFCore10.App -- menu`.
- `templates/portfolio-tutorial`: template local para novas migrações de pastas.

## Helper De Snippets De Código

- [x] Criar overloads em `TutorialConsole` para arquivo, tipo e membros selecionados.
- [x] Criar overload para ranges selecionados de um membro com `CodeExcerpt`.
- [x] Criar leitor textual mínimo em `CSharpCodePortfolio.Shared`, sem Roslyn.
- [x] Cobrir arquivo inteiro, tipo inteiro, construtor, property, método, ranges e erros claros com MSTest.
- [x] Migrar o Tutorial01 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial02 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial03 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial04 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial05 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial06 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial07 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial08 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial09 para snippets reais com ranges selecionados.
- [x] Migrar o Tutorial10 para snippets reais com ranges selecionados.
- [ ] Migrar os demais tutoriais somente após feedback do usuário, um tutorial por ciclo.

## Ordem De Migração Por Pasta

- [x] `AsyncAwaitTask`
- [x] `Client Server Socket Communication`
- [x] `Creating string Pipes using reverse Pipe Builder recursion`
- [x] `Server Client Communication with named Pipes`
- [x] `Using Anonymous Pipes for Communication between Threads`
- [x] `Lambda Validators`
- [x] `Replace If-Then-Else with Reflection and Attributes`
- [x] `Replace If-Then-Else for complex objects (using Reflection)`
- [x] `Replacing If-Then-Else using Pipe Builder structure (simple input types)`
- [x] `Replacing If-Then-Else for complex objects using Pipe Structure`
- [x] `Complete In-Memory IRepository without EF Library`
- [x] `InMemory EFCore without IRepository`
- [x] `InMemory EFCore using Services`
- [x] `Complete IRepository with InMemory EFCore`
- [x] `Dependency Injection .NET Core using Services`
- [x] `Dependency Inversion using Constructor and Property Dependency Injection Services`
- [x] `DotNet ZIP Library`
- [x] `Unit Tests using Reflection and Moq`
- [x] `ExposeInternalsToTest`
- [x] `MockHttpClient`
- [x] `MassTransitConsole`
- [x] `ASP.NET`
- [x] `RazorTest`
- [x] `RavenConnection`
- [x] `AspNet5 with Identity Framework`
- [x] `Minimum OpenID Server`
- [ ] `Sending Emails with Gmail`
- [ ] `Parallelism`

## Regra De Execucao Para Cada Pasta

1. Criar uma worktree por pasta e um `WORKTREE_GOAL.md` temporário com o objetivo.
2. Preservar o conceito principal da pasta em uma solução simples.
3. Preferir tutorial console `net10.0`; manter app web/API somente quando HTTP/auth for o próprio conceito.
4. Validar build, testes existentes e execução do tutorial migrado pelo menu raiz.
5. Commitar, mergear em `main`, remover a worktree e confirmar `git worktree list`.
6. Parar para feedback do usuário antes da próxima pasta.
