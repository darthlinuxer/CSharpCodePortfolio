# Tutorial 08 - DDD universitario e relacionamentos

Este tutorial usa um dominio de universidade para mostrar DDD tatico com EF
Core: aggregate roots, entidades internas, value objects, owned types,
relacionamentos por identidade e read models DTO.

O Tutorial07 continua sendo a referencia de TPH, TPT e TPC completos. Aqui a
heranca fica limitada a `Employee -> Professor / AdministrativeEmployee`, em
TPH, para manter o foco em fronteiras de dominio e mapeamento relacional.

## Estrutura

- `Domain/Institutional`: universidade, departamentos, funcionarios, campi e
  politica de demissao.
- `Domain/CourseCatalog`: cursos, plano de ensino, professor atribuido por ID e
  snapshots de catalogo.
- `Domain/EnrollmentManagement`: alunos, matriculas, semestre e nota.
- `Domain/Contracts`: snapshots que atravessam bounded contexts sem expor
  entidades de outro contexto.
- `Application/Commands`: command handlers, DTOs de caso de uso e interfaces
  finas para persistencia do sample.
- `Application/ReadModels`: DTOs e contratos de leitura para dashboard, cursos,
  alunos e carga docente.
- `Infrastructure/Persistence`: `DbContext`, tabelas e configuracoes EF Core.
- `Infrastructure/ReadModels`: consultas EF que implementam os contratos de
  leitura.

Mesmo dentro de um unico `.csproj`, a regra e simples: `Application` coordena,
`Domain` decide, `Infrastructure` mapeia. Nao ha MediatR, Wolverine, generic
repository ou interfaces especulativas. As interfaces existem so onde ajudam a
testar handlers/readers sem acoplar `Application` ao `DbContext`.

## Fronteiras DDD

`Course` guarda `DepartmentId` e `ProfessorId?`, nao `Department` nem
`Professor`. A regra de atribuicao recebe `ProfessorAssignmentSnapshot`, compara
departamento/status e retorna `Result`.

`Enrollment` guarda `StudentId`, `CourseId`, `Semester`, `EnrolledAtUtc` e
`FinalGrade`. Ele nao segura `Student` nem `Course`. `Student.RegisterForCourse`
recebe `CourseEnrollmentSnapshot` e os snapshots ja matriculados no semestre
para validar limite academico antes de mutar estado.

`University.DismissEmployee` recebe `AssignedCourseSnapshot`, entao a regra de
demissao nao precisa conhecer `Course`. Esse corte deixa o dominio mais perto
de bounded contexts reais sem mudar para varios projetos.

## Commands e DTOs

`RecreateUniversitySampleHandler` monta o sample via metodos de dominio e chama
uma interface fina de persistencia. A implementacao EF fica em `Infrastructure`.
Ele retorna
`Result<UniversitySampleDto>`, nao entidades de dominio. O DTO tem apenas dados
simples do caso de uso:

- `UniversityName`;
- `DepartmentCount`;
- `CourseCount`;
- `StudentCount`;
- `EnrollmentCount`.

Read models tambem retornam DTOs simples. `Application` define os contratos; as
consultas EF ficam em `Infrastructure/ReadModels`, fazem joins por IDs e nao
devolvem entities para o caller.

## Results e errors

Operacoes de dominio retornam `Result` ou `Result<T>`. `Result.Errors` carrega
uma lista de `DomainError` com `Code` e `Message`. Uma regra violada vira erro
de dominio; varias regras violadas voltam juntas.

Excecoes ficam para falhas raras: bug de chamada, infraestrutura,
materializacao EF com dado invalido ou self-check interno do tutorial. Regra de
negocio violada nao e excecao.

## Value objects

O dominio armazena value objects, nao primitivos. Boundaries e factories podem
receber `string?`, `int`, `DateTime` ou DTOs brutos para acumular todos os
erros de input antes de mutar estado.

Todos os value objects sao fechados, com construtor privado. Entrada normal
passa por `Create(...)` e retorna `Result<T>`. Novos IDs passam por `New()`. A
materializacao EF passa por `FromStorage(...).RequireValue()`: dado invalido no
banco e falha excepcional de persistencia, nao regra de negocio do usuario.

## EF Core

O schema continua relacional mesmo com bounded contexts:

- `Course -> Syllabus` usa `OwnsOne`;
- `University -> Campuses` usa `OwnsMany`;
- `Department`, `Employee`, `Course`, `Student` e `Enrollment` tem tabelas
  proprias quando identidade ou payload justificam;
- FKs entre contexts ficam como IDs no dominio e relacionamentos no EF;
- TPH de funcionarios usa checks para exigir `DepartmentId` em professores e
  `Role` em administrativos.

Read services usam `AsNoTracking()` e `Select(...)`; quando a leitura atravessa
contexts, fazem joins por `Id`. Isso mantem queries didaticas sem contaminar os
aggregates com navegacoes externas.

## Execucao

```bash
dotnet run --project src/EFCore10.App --no-restore -- run 08
```

A saida mostra self-checks de dominio, SQL das queries DTO, estrategia de
mapeamento EF, DDL real do SQLite e validacao do schema.
