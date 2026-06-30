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
- `Application/Persistence`: contratos de repositório por aggregate, unit of
  work monádica e executor transacional.
- `Infrastructure`: `Tutorial30DbContext` com repositórios EF por aggregate,
  `EfTutorial30UnitOfWork`, `EfTransactionalExecution`, tradutores EF de erro
  esperado e dispatcher outbox in-process.
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
2. O repositório EF adiciona o aggregate ao `Tutorial30DbContext`.
3. `EfTutorial30UnitOfWork.SaveEntitiesAsync` abre uma transação via
   `EfTransactionalExecution`, salva o aggregate e publica
   `UserAccountRegisteredDomainEvent` no bus local.
4. O handler local de Identity publica `UserRegisteredIntegrationEvent` pela
   porta `IIntegrationEventBus`.
5. A implementação `OutboxIntegrationEventBus` grava a mensagem na outbox
   durante a mesma transação.
6. A unit of work faz o segundo `SaveChangesAsync`, commita e só então limpa os
   domain events dos aggregates.
7. O dispatcher in-process lê a outbox e entrega a mensagem a consumidores
   registrados por contrato.
8. Ordering atualiza `CustomerDirectory` e cria `Order` usando apenas
   `CustomerId`.
9. `ConfirmOrderService` confirma `Order`; o domain handler de Ordering publica
   `OrderConfirmedIntegrationEvent`.
10. Billing consome o evento de integração e cria uma `Invoice` uma única vez.

## O que ficou fora

Sem MediatR, broker real, event sourcing, saga ou CQRS pesado. O repositório
genérico é mínimo e só cobre aggregate root (`FindById`, `Add`, `Remove`);
queries e projeções continuam em portas próprias.

O diretório `Traditional` fica como anti-exemplo imperativo. O restante do
Tutorial30 é protegido por teste arquitetural contra condicionais imperativos.

## Executar

```bash
dotnet test tests/CSharpCodePortfolio.Tutorials.Tutorial30.Tests/CSharpCodePortfolio.Tutorials.Tutorial30.Tests.csproj --no-restore
dotnet run --project src/CSharpCodePortfolio.App --no-restore -- run 30
```
