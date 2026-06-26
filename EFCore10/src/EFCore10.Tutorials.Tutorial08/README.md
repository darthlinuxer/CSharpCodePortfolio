# Tutorial 08 - DDD universitario e relacionamentos

Este tutorial usa um dominio de universidade para mostrar DDD tatico com EF
Core: aggregate roots, entidades internas, value objects, owned types e
relacionamentos com identidade propria, services e read models DTO.

O Tutorial07 continua sendo a referencia de TPH, TPT e TPC completos. Aqui a
heranca fica limitada a `Employee -> Professor / AdministrativeEmployee`, em
TPH, para demonstrar o tipo de tabela configurada sem transformar o exemplo em
um tutorial de heranca.

## Dominio

- `University` e aggregate root: abre departamentos, contrata funcionarios,
  possui campi e demite funcionarios.
- `Department` e entidade interna da universidade: tem identidade e professores,
  mas nao deve virar ponto de entrada/repository separado no tutorial.
- `Course` e aggregate root: tem pontos academicos, plano de ensino e professor
  opcional.
- `Student` e aggregate root: registra matriculas e impede mais de 40 pontos no
  mesmo semestre.
- `Enrollment` e join entity com payload: semestre, data UTC e nota final
  opcional.

Aggregate root nao significa "uma tabela so". DDD define fronteiras de
consistencia e acesso ao dominio. O banco relacional ainda pode precisar de
tabelas para entidades internas, joins e owned collections.

## Estrutura centrada no dominio

- `Domain/` contem aggregates, entidades, value objects, erros e policies.
- `Application/` contem services de caso de uso e read services.
- `Persistence/` contem somente `DbContext`, tabelas e configuracoes EF Core.

O modelo protege seus dados: colecoes usam backing fields privados e expõem
`IReadOnlyCollection<T>`. Mudancas passam por metodos de dominio, nao por
`List<T>.Add(...)` externo.

## Services

- `UniversityApplicationService` orquestra o caso de uso do tutorial:
  recria banco, cria o cenario e chama metodos do dominio.
- `ProfessorDismissalPolicy` e domain service porque a regra de demissao cruza
  `University`, `Professor` e `Course`.
- `UniversityDashboardReadService` monta o painel institucional.
- `CourseAnalyticsReadService` calcula demanda, professor, departamento,
  matriculas e medias por curso.
- `StudentProgressReadService` calcula carga discente por semestre.
- `FacultyWorkloadReadService` calcula carga docente por professor e
  departamento.

Read service nao e domain service. Ele responde perguntas de leitura, usa EF
Core e devolve DTO. Regras como limite de pontos, demissao e duplicidade ficam
no dominio, onde podem ser exercitadas sem banco.

Nao ha interface, MediatR, CQRS framework ou generic repository. Para este
tutorial, isso seria mais estrutura que valor.

## Errors

`DomainException` carrega `Code` e `Message`. A mensagem ensina; o codigo e
estavel para self-checks e possivel UI/API futura.

## Value objects

Os aggregates nao expõem strings, inteiros, `Guid` ou `DateTime` crus nas suas
APIs principais. Exemplos:

- `UniversityId`, `DepartmentId`, `CourseId`, `StudentId`, `EmployeeId`;
- `PersonName`, `EmailAddress`, `CourseCode`, `CourseTitle`;
- `CreditPoints`, `Semester`, `UtcDateTime`, `Grade`.

O EF Core salva esses tipos como colunas escalares usando `HasConversion(...)`.

## OwnsOne, OwnsMany, HasOne e HasMany

- `Course -> Syllabus` usa `OwnsOne`: o plano de ensino nao tem identidade nem
  ciclo de vida sem o curso, entao suas colunas ficam em `Courses`.
- `University -> Campuses` usa `OwnsMany`: campus pertence somente a uma
  universidade, mas a colecao precisa de linhas em `UniversityCampuses`.
- `University -> Departments`, `University -> Employees`, `Department ->
  Professors`, `Course -> Professor` e `Student/Course -> Enrollment` usam
  `HasOne`/`HasMany`, porque esses objetos tem identidade propria ou payload de
  relacionamento. Um professor pode estar associado a varios cursos.
- `Professor` nao e owned type: professor e funcionario, tem identidade,
  pertence a departamento e pode ser atribuido a curso.

## Shadow properties

Algumas FKs aparecem no banco, mas nao aparecem no modelo de dominio:

- `Department -> University` usa shadow `UniversityId`;
- `Employee -> University` usa shadow `UniversityId`;
- `Professor -> Department` usa shadow `DepartmentId`;
- `Course -> Professor` usa shadow `ProfessorId`.

Essas propriedades sao indicadas aqui porque o dominio ja navega por objetos:
`Department.University`, `Employee.University`, `Professor.Department` e
`Course.Professor`. Expor tambem o ID duplicaria a mesma associacao e abriria
espaco para estado inconsistente.

`Enrollment` e a excecao proposital: `StudentId`, `CourseId` e `Semester`
formam a chave da matricula e participam das regras de duplicidade, entao fazem
parte do modelo.

## DTO queries

O tutorial usa read models para consultas avancadas:

- `UniversityDashboardDto`;
- `CourseAnalyticsDto`;
- `StudentProgressDto`;
- `FacultyWorkloadDto`.

DTOs usam tipos simples porque sao contratos de leitura, nao objetos de dominio.
As queries usam `AsNoTracking()` e `Select(...)` para buscar somente o que o
relatorio precisa. `Include` nao aparece nas leituras porque o tutorial nao
materializa grafo para alterar estado; ele projeta colunas.

As consultas agregam no banco quando o EF Core traduz bem, como contagem de
matriculas por curso. Quando value objects dificultam a traducao, como a media
de `Grade?`, o read service busca linhas minimas e finaliza o calculo em
memoria. Esse limite fica visivel no codigo sem virar raw SQL ou keyless entity
type.

O mapeamento tambem declara indices pequenos para as consultas ensinadas:

- `Courses.Code`;
- `Courses.ProfessorId`;
- `Departments.UniversityId + Name`;
- `Employees.UniversityId`;
- `Employees.EmployeeType + Status`;
- `Employees.DepartmentId`;
- `Enrollments.Semester + CourseId`.

## Como estudar `Persistence/Configurations`

As classes de `Configurations/` sao a parte central do tutorial. O jeito certo
de le-las nao e decorar Fluent API, mas perguntar por que cada comando existe.

Perguntas socraticas para qualquer configuracao:

- Que tipo de coisa estou mapeando: aggregate root, entidade interna, owned
  type, join entity ou subtipo?
- Esse tipo tem identidade propria ou vive dentro de outro objeto?
- A coluna pertence ao modelo de dominio ou e apenas infraestrutura relacional?
- O EF deve acessar a colecao pelo backing field para nao furar invariantes?
- A delecao deve remover dependentes ou bloquear a operacao?
- Qual consulta real justifica este indice?

### `UniversityConfiguration`

- `ToTable(Tables.Universities)` responde: `University` tem tabela propria? Sim,
  porque e aggregate root.
- `HasTableMapping(ConcreteTable)` nao muda o EF; e uma anotacao do tutorial para
  explicar a estrategia na saida.
- `Navigation(...).UsePropertyAccessMode(Field)` responde: quem pode alterar
  colecoes? O dominio. O EF preenche os backing fields privados sem expor
  `List<T>`.
- `HasKey(Id)` fixa a identidade relacional do aggregate.
- `Property(Id).HasConversion(...).ValueGeneratedNever()` preserva `UniversityId`
  como value object e deixa o dominio gerar o identificador.
- `Property(Name).HasConversion(...).HasMaxLength(...).IsRequired()` persiste
  `UniversityName` como texto limitado, sem aceitar valor ausente.
- `OwnsMany(Campuses)` responde: campus existe sem universidade? Nao. A colecao
  ganha tabela por ser lista, mas continua owned.
- `WithOwner().HasForeignKey(UniversityId)` cria o vinculo relacional com o
  owner.
- `Property<UniversityId>(UniversityId)` cria shadow FK; o dominio nao precisa
  expor esse ID porque campus nao vive fora de `University`.
- `HasKey(UniversityId, Id)` garante unicidade do campus dentro da universidade,
  nao globalmente.

### `DepartmentConfiguration`

- `ToTable(Tables.Departments)` cria tabela porque `Department` tem identidade,
  mesmo sendo entidade interna de `University`.
- `HasTableMapping(InternalEntity)` ensina que entidade interna pode ter tabela
  sem virar aggregate root.
- `Navigation(Professors).UsePropertyAccessMode(Field)` preserva a colecao
  controlada pelo dominio.
- `HasKey(Id)` e `Property(Id).HasConversion(...).ValueGeneratedNever()` mantem
  `DepartmentId` como identidade criada no dominio.
- `Property(Name).HasConversion(...).HasMaxLength(...).IsRequired()` aplica o
  value object `DepartmentName` ao schema.
- `Property<UniversityId>(UniversityId)` cria shadow FK porque a entidade ja tem
  a navegacao `University`.
- `HasIndex(UniversityId, Name).IsUnique()` reforca a regra de nome unico por
  universidade e ajuda as consultas por instituicao.
- `HasOne(University).WithMany(Departments).HasForeignKey(UniversityId)` modela
  que departamento pertence a uma universidade.
- `OnDelete(Cascade)` remove departamentos quando a universidade e removida no
  banco recriado pelo tutorial.

### `EmployeeConfiguration`, `ProfessorConfiguration` e
`AdministrativeEmployeeConfiguration`

- `ToTable(Tables.Employees)` e `UseTphMappingStrategy()` colocam a hierarquia
  curta de funcionarios em uma tabela.
- `HasDiscriminator("EmployeeType").HasValue<...>()` diferencia `Professor` e
  `AdministrativeEmployee` sem criar TPT/TPC aqui.
- `HasKey(Id)` e `Property(Id).HasConversion(...).ValueGeneratedNever()`
  preservam `EmployeeId`.
- `Property(Name/Email/HiredAtUtc/DismissedAtUtc/Status).HasConversion(...)`
  guarda value objects como escalares e reconstrui tipos validados ao ler.
- `Property<UniversityId>(UniversityId)` e `Property<DepartmentId>(DepartmentId)`
  sao shadow FKs: o banco precisa das colunas; o dominio trabalha com
  `University` e `Department`.
- `HasIndex(UniversityId)`, `HasIndex(EmployeeType, Status)` e
  `HasIndex(DepartmentId)` documentam os acessos usados por dashboard e carga
  docente.
- `HasOne(University).WithMany(Employees).OnDelete(Restrict)` evita apagar uma
  universidade deixando funcionarios inconsistentes.
- `HasOne(Department).WithMany(Professors).OnDelete(Restrict)` evita excluir
  departamento ainda usado por professor.
- `AdministrativeEmployee.Role` fica nullable na tabela TPH porque so existe
  nessa ramificacao, mas continua `StaffRole` no dominio.

### `CourseConfiguration`

- `ToTable(Tables.Courses)` indica aggregate root com tabela concreta.
- `Navigation(Enrollments).UsePropertyAccessMode(Field)` permite materializar
  matriculas sem liberar alteracao direta da lista.
- `Ignore(Students)` evita mapear uma colecao derivada de `Enrollments` como se
  fosse outro relacionamento.
- `HasKey(Id)` e `Property(Id).HasConversion(...).ValueGeneratedNever()` mantem
  `CourseId` no dominio.
- `Property(Title/Code/CreditPoints).HasConversion(...).IsRequired()` salva value
  objects como colunas simples.
- `HasIndex(Code).IsUnique()` torna visivel a unicidade natural do codigo do
  curso.
- `Property<EmployeeId?>(ProfessorId)` cria shadow FK opcional: curso pode existir
  antes de receber professor.
- `OwnsOne(Syllabus)` responde: plano de ensino tem identidade propria? Nao.
  Entao suas colunas ficam em `Courses`.
- `HasColumnName("SyllabusSummary")` e `HasColumnName("SyllabusOutcomes")`
  explicam no DDL onde o owned type foi armazenado.
- `HasOne(Professor).WithMany().HasForeignKey(ProfessorId)` permite varios cursos
  por professor.
- `OnDelete(Restrict)` protege cursos de ficarem com professor removido
  acidentalmente.

### `StudentConfiguration` e `EnrollmentConfiguration`

- `Student.ToTable(Tables.Students)` mapeia outro aggregate root.
- `Student.Ignore(Courses)` evita duplicar o relacionamento ja representado por
  `Enrollment`.
- `Enrollment.ToTable(Tables.Enrollments)` torna a relacao aluno-curso uma join
  entity com payload.
- `HasKey(StudentId, CourseId, Semester)` responde: o que torna uma matricula
  unica? Aluno, curso e semestre.
- `Property(StudentId/CourseId/Semester).HasConversion(...)` deixa esses IDs no
  dominio porque participam da regra de duplicidade.
- `HasIndex(Semester, CourseId)` atende a consulta de progresso por semestre e
  demanda por curso.
- `Property(EnrolledAtUtc)` e `Property(FinalGrade).HasPrecision(4, 2)` salvam o
  payload da matricula.
- `HasOne(Student).WithMany(Enrollments).OnDelete(Cascade)` e
  `HasOne(Course).WithMany(Enrollments).OnDelete(Cascade)` removem matriculas
  quando um dos lados some, porque a linha nao faz sentido sozinha.

## Execucao

```bash
dotnet run --project src/EFCore10.App --no-restore -- run 08
```

A saida mostra:

- self-checks das regras de dominio;
- SQL de queries DTO geradas pelo read service;
- metadados didaticos de mapeamento (`TPH`, `ConcreteTable`, `InternalEntity`,
  `OwnsOne`, `OwnsMany`, `JoinEntity`);
- DDL real lido de `sqlite_master`, incluindo indices;
- DTOs de dashboard, demanda de cursos, progresso discente e carga docente;
- validacao de schema para garantir que nao sobraram tabelas do dominio antigo
  de vendas/auditoria.
