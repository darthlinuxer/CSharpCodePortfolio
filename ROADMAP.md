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
- [ ] Migrar pastas de topo restantes, uma pasta por vez.

## Arquitetura Alvo

- `CSharpCodePortfolio.App`: CLI raiz com comandos `menu`, `list` e `run <id-or-slug>`.
- `CSharpCodePortfolio.Shared`: UI de console compartilhada para tutoriais do portfolio.
- `CSharpCodePortfolio.Tutorials.Abstractions`: contrato mínimo de descoberta por atributo e execução.
- `EFCore10`: permanece como workspace independente; o host raiz executa o menu dele via `dotnet run --project EFCore10/src/EFCore10.App -- menu`.
- `templates/portfolio-tutorial`: template local para novas migrações de pastas.

## Ordem De Migração Por Pasta

- [x] `AsyncAwaitTask`
- [x] `Client Server Socket Communication`
- [x] `Creating string Pipes using reverse Pipe Builder recursion`
- [x] `Server Client Communication with named Pipes`
- [ ] `Using Anonymous Pipes for Communication between Threads`
- [ ] `Lambda Validators`
- [ ] `Replace If-Then-Else with Reflection and Attributes`
- [ ] `Replace If-Then-Else for complex objects (using Reflection)`
- [ ] `Replacing If-Then-Else using Pipe Builder structure (simple input types)`
- [ ] `Replacing If-Then-Else for complex objects using Pipe Structure`
- [ ] `Complete In-Memory IRepository without EF Library`
- [ ] `InMemory EFCore without IRepository`
- [ ] `InMemory EFCore using Services`
- [ ] `Complete IRepository with InMemory EFCore`
- [ ] `Dependency Injection .NET Core using Services`
- [ ] `Dependency Inversion using Constructor and Property Dependency Injection Services`
- [ ] `DotNet ZIP Library`
- [ ] `Unit Tests using Reflection and Moq`
- [ ] `ExposeInternalsToTest`
- [ ] `MockHttpClient`
- [ ] `MassTransitConsole`
- [ ] `ASP.NET`
- [ ] `RazorTest`
- [ ] `RavenConnection`
- [ ] `AspNet5 with Identity Framework`
- [ ] `Minimum OpenID Server`
- [ ] `Sending Emails with Gmail`
- [ ] `Parallelism`

## Regra De Execucao Para Cada Pasta

1. Criar uma worktree por pasta e um `WORKTREE_GOAL.md` temporário com o objetivo.
2. Preservar o conceito principal da pasta em uma solução simples.
3. Preferir tutorial console `net10.0`; manter app web/API somente quando HTTP/auth for o próprio conceito.
4. Validar build, testes existentes e execução do tutorial migrado pelo menu raiz.
5. Commitar, mergear em `main`, remover a worktree e confirmar `git worktree list`.
6. Parar para feedback do usuário antes da próxima pasta.
