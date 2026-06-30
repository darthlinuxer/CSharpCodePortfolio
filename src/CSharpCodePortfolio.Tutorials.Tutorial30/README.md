# Tutorial 30 - LanguageExt.Core + DDD monádico

Este tutorial mostra um esqueleto DDD pequeno com `LanguageExt.Core`, bounded
contexts e comunicação por eventos:

- `Option<T>` para ausência válida.
- `Either<DomainError,T>` para falhas esperadas.
- Aggregates independentes por bounded context.
- Domain events internos.
- Integration events como published language entre contexts.
- Outbox persistida simples e dispatcher in-process como broker emulado.

## Estrutura

- `SharedKernel`: building blocks mínimos (`IEntity`, `IAggregate`,
  `AbstractAggregate`, `DomainError`, `AbstractDomainEvent`, `Timestamp`).
- `Contexts/Identity`: `UserAccount`, cadastro e evento
  `UserAccountRegisteredDomainEvent`.
- `Contexts/Ordering`: `Order`, `OrderLine`, `CustomerDirectory`, commands de
  criação/confirmação e `OrderConfirmedDomainEvent`.
- `Contexts/Billing`: `Invoice`, handler idempotente com resultado explícito
  `Created`/`AlreadyHandled`.
- `Integration/Messaging`: contratos puros de publicação e consumo de eventos
  de integração.
- `Integration/Events`: contratos publicados entre bounded contexts.
- `Integration/Outbox`: envelope persistido para mensagens pendentes.
- `Infrastructure`: `Tutorial30DbContext`, `EfTutorial30UnitOfWork`,
  tradutores EF de erro esperado e dispatcher outbox in-process.
- `Presentation`: tradução HTTP fina de `Either` para `IResult`.

## Fronteiras DDD

`Identity` é dono de `UserAccount`. `Ordering` não referencia esse aggregate:
usa `CustomerId` local, alimentado por `UserRegisteredIntegrationEvent`.

`Billing` não referencia `Order`: cria `Invoice` a partir do contrato publicado
`OrderConfirmedIntegrationEvent`. O payload é mínimo: ids e valor total.

Domain events ficam dentro do próprio bounded context e são enviados por
`IInMemoryDomainEventBus`. Handlers locais podem publicar integration events
por `IIntegrationEventBus`; a implementação de infraestrutura atual grava na
outbox e pode ser trocada por RabbitMQ, fila ou broker real depois.

## Fluxo demonstrado

1. `RegisterUserService` cria `UserAccount`.
2. `EfTutorial30UnitOfWork` salva o aggregate e publica
   `UserAccountRegisteredDomainEvent` no bus local.
3. O handler local de Identity publica `UserRegisteredIntegrationEvent` pela
   porta `IIntegrationEventBus`.
4. A implementação `OutboxIntegrationEventBus` grava a mensagem na outbox
   durante a mesma transação.
5. O dispatcher in-process lê a outbox e entrega a mensagem a consumidores
   registrados por contrato.
6. Ordering atualiza `CustomerDirectory` e cria `Order` usando apenas
   `CustomerId`.
7. `ConfirmOrderService` confirma `Order`; o domain handler de Ordering publica
   `OrderConfirmedIntegrationEvent`.
8. Billing consome o evento de integração e cria uma `Invoice` uma única vez.

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
