# Tutorial 30 - LanguageExt.Core + DDD monádico

Este tutorial mostra um esqueleto DDD pequeno com `LanguageExt.Core`, bounded
contexts e comunicação por eventos:

- `Option<T>` para ausência válida.
- `Either<DomainError,T>` para falhas esperadas.
- Aggregates independentes por bounded context.
- Domain events internos.
- Integration events como published language entre contexts.
- Outbox persistida simples e dispatcher in-process para o tutorial.

## Estrutura

- `SharedKernel`: building blocks mínimos (`IEntity`, `IAggregate`,
  `AbstractAggregate`, `DomainError`, `AbstractDomainEvent`, `Timestamp`).
- `Contexts/Identity`: `UserAccount`, cadastro e evento
  `UserRegisteredIntegrationEvent`.
- `Contexts/Ordering`: `Order`, `OrderLine`, `CustomerDirectory`, commands de
  criação/confirmação e evento `OrderConfirmedIntegrationEvent`.
- `Contexts/Billing`: `Invoice`, handler idempotente com resultado explícito
  `Created`/`AlreadyHandled`.
- `Integration`: contratos publicados, outbox e dispatcher sem broker real.
- `Infrastructure`: `Tutorial30DbContext`, usado só como adapter de persistência.
- `Presentation`: tradução HTTP fina de `Either` para `IResult`.

## Fronteiras DDD

`Identity` é dono de `UserAccount`. `Ordering` não referencia esse aggregate:
usa `CustomerId` local, alimentado por `UserRegisteredIntegrationEvent`.

`Billing` não referencia `Order`: cria `Invoice` a partir do contrato publicado
`OrderConfirmedIntegrationEvent`. O payload é mínimo: ids e valor total.

## Fluxo demonstrado

1. `RegisterUserService` cria `UserAccount`.
2. `EfIntegrationOutbox` grava `UserRegisteredIntegrationEvent` na mesma
   transação.
3. `InProcessOutboxDispatcher` entrega o evento ao ACL de Ordering.
4. `PlaceOrderService` cria `Order` usando apenas `CustomerId`.
5. `ConfirmOrderService` confirma `Order` e grava `OrderConfirmedIntegrationEvent`.
6. Billing consome o evento e cria uma `Invoice` uma única vez.

## O que ficou fora

Sem `GenericRepository<T>`, MediatR, broker real, event sourcing, saga ou CQRS
pesado. Esses padrões entram só quando houver regra real que pague o custo.

O diretório `Traditional` fica como anti-exemplo imperativo. O restante do
Tutorial30 é protegido por teste arquitetural contra condicionais imperativos.

## Executar

```bash
dotnet test tests/CSharpCodePortfolio.Tutorials.Tutorial30.Tests/CSharpCodePortfolio.Tutorials.Tutorial30.Tests.csproj --no-restore
dotnet run --project src/CSharpCodePortfolio.App --no-restore -- run 30
```
