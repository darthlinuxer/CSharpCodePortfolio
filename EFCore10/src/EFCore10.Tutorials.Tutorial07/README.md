# Tutorial 07 - DDD-rich inheritance mapping

Este tutorial usa um pequeno catalogo educacional para comparar as tres
estrategias de heranca do EF Core 10: TPH, TPT e TPC.

A regra importante e propositalmente simples: o dominio decide se um recurso
pode ser publicado; o EF Core decide apenas como a hierarquia vira tabelas.

## Modelo de dominio

`LearningResource` e o aggregate root abstrato. Ele concentra os dados comuns:

- `ResourceId`, gerado com `Guid.CreateVersion7()`;
- `ResourceTitle`, `InstructorName` e `LearningLevel` como value objects;
- `CreatedOnUtc` e `PublishedOnUtc`;
- `Rename`, `Publish` e `EnsureCanPublish`.

Os tipos concretos adicionam regras especificas:

- `ArticleResource` so publica com pelo menos 600 palavras.
- `VideoResource` so publica com pelo menos 5 minutos.
- `LiveWorkshopResource` so publica com pelo menos 5 vagas.

Essas regras ficam no modelo, nao em setters publicos nem em codigo de
persistencia. O tutorial cria um artigo curto e prova que `Publish` falha antes
de qualquer acesso ao banco.

## Value objects

Os value objects usam records e record structs:

- `ResourceId`: identidade forte baseada em `Guid`.
- `ResourceTitle`: normaliza espacos e exige tamanho valido.
- `InstructorName`: normaliza espacos e exige tamanho valido.
- `LearningLevel`: aceita apenas `Beginner`, `Intermediate` ou `Advanced`.
- `WordCount`, `VideoDuration` e `SeatLimit`: garantem valores positivos.

No mapeamento, todos viram colunas escalares por `HasConversion`. Assim o
modelo continua expressivo em C#, mas o SQLite recebe tipos simples.

## Table per Hierarchy (TPH)

TPH grava toda a hierarquia em uma unica tabela: `LearningResources`.

O EF Core adiciona a coluna `ResourceType` para saber se cada linha representa
`ArticleResource`, `VideoResource` ou `LiveWorkshopResource`. As colunas
especificas dos tipos concretos ficam na mesma tabela e precisam aceitar `NULL`
quando a linha pertence a outro tipo.

Vantagem: schema simples e consulta polimorfica sem joins.

Custo: tabela mais larga e colunas nulas para dados que pertencem so a alguns
tipos.

## Table per Type (TPT)

TPT grava os dados comuns em `LearningResources` e os dados especificos em
`Articles`, `Videos` e `LiveWorkshops`.

Cada tabela derivada usa a mesma chave da tabela base. Para montar um objeto
concreto, o EF Core precisa juntar a tabela base com as tabelas derivadas.

Vantagem: schema normalizado e proximo do desenho OO.

Custo: consultas polimorficas exigem joins.

## Table per Concrete Type (TPC)

TPC nao cria tabela para o tipo abstrato `LearningResource`.

Cada tipo concreto tem sua propria tabela completa:

- `Articles`
- `Videos`
- `LiveWorkshops`

As colunas herdadas (`Id`, `Title`, `Instructor`, `Level`, datas) aparecem em
cada tabela concreta.

Vantagem: consultar um tipo concreto nao precisa juntar tabela base.

Custo: as colunas comuns sao duplicadas entre tabelas. Por isso o tutorial usa
IDs gerados no dominio com `Guid.CreateVersion7()`, evitando depender de
sequencias compartilhadas no SQLite.

## Execucao

```bash
dotnet run --project src/EFCore10.App --no-restore -- run 07
```

A saida mostra:

- a regra DDD falhando em um draft invalido;
- o SQL gerado para a consulta polimorfica em cada estrategia;
- o DDL real lido de `sqlite_master`;
- as tabelas criadas e os tipos materializados pelo EF Core.
