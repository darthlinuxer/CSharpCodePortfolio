# Tutorial08 - fluxo de camadas

## Veredito

O tutorial usa camadas dentro de um unico `.csproj`. A separacao e por pasta,
namespace e checks de arquitetura, nao por assembly. O dominio ficou livre de
EF Core e de infraestrutura; bounded contexts nao seguram entidades de outro
contexto. O acoplamento com SQLite/EF fica em `Infrastructure`.

| Camada | Responsabilidade | Pode depender de | Observacao |
| --- | --- | --- | --- |
| `Domain` | Regras, aggregates, VOs, `Result` | BCL e tipos do proprio dominio | Sem EF, sem `DbContext`, sem annotations de persistencia |
| `Application` | Commands, DTOs e interfaces finas | `Domain` | Nao conhece `Infrastructure` nem EF |
| `Infrastructure` | EF Core, SQLite, queries e storage | `Application`, `Domain` | Implementa contratos finos com `UniversityContext` |
| `Runner` | Composition root do tutorial | Todas as camadas | Monta EF e injeta infra nos handlers/readers |

## Dependencias

```mermaid
flowchart LR
    runner["Runner\nRelationshipMappingTutorial"]

    subgraph app["Application"]
        commands["Commands\nRecreateUniversitySampleHandler"]
        readContracts["ReadModels\nDTOs + interfaces"]
        sampleStore["IUniversitySampleStore"]
    end

    subgraph domain["Domain"]
        institutional["Institutional\nUniversity, Department, Employee"]
        catalog["CourseCatalog\nCourse, Syllabus"]
        enrollment["EnrollmentManagement\nStudent, Enrollment"]
        contracts["Contracts\nsnapshots + IDs"]
        common["Common\nVOs, Result, DomainError"]
    end

    subgraph infra["Infrastructure"]
        persistence["Persistence\nUniversityContext + configs"]
        readers["ReadModels\nEF joins + projections"]
    end

    sqlite[("SQLite")]

    runner --> commands
    runner --> readers
    runner --> persistence

    commands --> institutional
    commands --> catalog
    commands --> enrollment
    commands --> contracts
    commands --> sampleStore

    readers -. implements .-> readContracts
    persistence -. implements .-> sampleStore

    readers --> persistence
    persistence --> sqlite
    persistence --> domain

    catalog --> contracts
    enrollment --> contracts
    institutional --> contracts
    domain --> common

    domain -. proibido .-> infra
    domain -. proibido .-> app
    catalog -. "proibido: Department/Professor entity" .-> institutional
    enrollment -. "proibido: Course entity" .-> catalog

    classDef runner fill:#eef2ff,stroke:#4f46e5,color:#111827;
    classDef app fill:#ecfeff,stroke:#0891b2,color:#111827;
    classDef domain fill:#f0fdf4,stroke:#16a34a,color:#111827;
    classDef infra fill:#fff7ed,stroke:#ea580c,color:#111827;
    classDef db fill:#f8fafc,stroke:#475569,color:#111827;
    classDef forbidden fill:#fee2e2,stroke:#dc2626,stroke-dasharray:5 5,color:#111827;

    class runner runner;
    class commands,readContracts,sampleStore app;
    class institutional,catalog,enrollment,contracts,common domain;
    class persistence,readers infra;
    class sqlite db;
```

## Fluxo do command

```mermaid
sequenceDiagram
    participant Runner as Runner
    participant Handler as Application Handler
    participant Domain as Domain Aggregates
    participant Store as Infrastructure Store
    participant EF as UniversityContext
    participant DB as SQLite

    Runner->>Handler: RecreateUniversitySampleCommand
    Handler->>Domain: factories + domain behavior
    Domain-->>Handler: Result + aggregate graph
    Handler->>Store: RecreateAsync(university, courses, students)
    Store->>EF: EnsureDeleted/EnsureCreated/Add/SaveChanges
    EF->>DB: schema + rows
    Store-->>Handler: completed
    Handler-->>Runner: Result<UniversitySampleDto>
```

## Leitura

Read models seguem outro caminho: o runner chama contratos de leitura, as
implementacoes em `Infrastructure/ReadModels` fazem joins por IDs no EF e
retornam DTOs simples. Nenhuma query devolve entidade de dominio como contrato
do consumidor.

## Checks

```bash
rg -n "Microsoft.EntityFrameworkCore|UniversityContext|Infrastructure|Persistence" src/EFCore10.Tutorials.Tutorial08/Domain src/EFCore10.Tutorials.Tutorial08/Application -g '*.cs'
rg -n "System.ComponentModel.DataAnnotations.Schema|\\[NotMapped\\]" src/EFCore10.Tutorials.Tutorial08/Domain -g '*.cs'
rg -n "namespace EFCore10\\.Tutorials\\.Tutorial08\\.Persistence|using EFCore10\\.Tutorials\\.Tutorial08\\.Persistence" src/EFCore10.Tutorials.Tutorial08 -g '*.cs'
```
