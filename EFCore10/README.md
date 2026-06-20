# EFCore10

Este workspace usa templates do `dotnet new` sem instalar esses templates no
catálogo global do usuário.

## Diferença para o tutorial da Microsoft

O tutorial da Microsoft para templates de projeto usa este fluxo básico:

```bash
dotnet new install ./
dotnet new <shortName> --name MyProject
```

Esse fluxo é adequado quando o template deve ficar disponível em qualquer
diretório da máquina. Depois de uma instalação normal, `dotnet new list` pode
mostrar o template a partir de outros projetos do mesmo usuário.

Este repositório faz diferente de propósito. O template de tutorial só faz
sentido dentro deste workspace, porque o projeto gerado precisa entrar na
solution e ser referenciado por `EFCore10.App`.

## Hive local de templates

O repositório usa um hive local:

```text
.dotnet-template-hive/
```

Esse diretório é gerado pelo próprio mecanismo de templates do .NET. Ele não é
configuração escrita manualmente e fica ignorado pelo Git. O wrapper abaixo é o
equivalente local de `dotnet new`:

```bash
./scripts/dotnet-new-local.sh <argumentos-do-dotnet-new>
```

Internamente, o wrapper executa:

```bash
dotnet new --debug:custom-hive "$repo_root/.dotnet-template-hive" "$@"
```

A opção `--debug:custom-hive` faz o mecanismo de templates usar o hive deste
workspace em vez do hive global do usuário.

Não existe um comando separado como `dotnet new create-hive`. O hive nasce na
primeira execução de `dotnet new` que usa `--debug:custom-hive <caminho>`. O
comando pode ser `list`, `install` ou a criação de um template. O .NET CLI cria
as pastas internas automaticamente.

Para configurar isso do zero em outro workspace:

1. Escolha o caminho do hive local. Neste repo usamos:

   ```bash
   .dotnet-template-hive
   ```

2. Ignore esse diretório no Git:

   ```gitignore
   .dotnet-template-hive/
   ```

3. Crie um wrapper para sempre chamar `dotnet new` com o hive local:

   ```bash
   mkdir -p scripts
   cat > scripts/dotnet-new-local.sh <<'SCRIPT'
   #!/usr/bin/env bash
   set -euo pipefail

   repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
   hive="$repo_root/.dotnet-template-hive"

   exec dotnet new --debug:custom-hive "$hive" "$@"
   SCRIPT
   chmod +x scripts/dotnet-new-local.sh
   ```

4. Execute qualquer comando pelo wrapper. Esse primeiro comando já cria a
   estrutura interna do hive:

   ```bash
   ./scripts/dotnet-new-local.sh list
   ```

5. Depois que existir uma pasta `templates/` com ao menos um template válido,
   instale os templates no hive local:

   ```bash
   ./scripts/dotnet-new-local.sh install ./templates --force
   ```

Neste repositório, `scripts/new-tutorial.sh` também faz o passo de instalação
local antes de criar um tutorial, então o usuário não precisa criar ou instalar
o hive manualmente para o fluxo principal.

## Template atual de tutorial

O template atual fica em:

```text
templates/efcore10-tutorial/
  .template.config/template.json
  EFCore10.Tutorials.TutorialTemplate.csproj
  TutorialClass.cs
  appsettings.json
```

Os arquivos em `templates/efcore10-tutorial/` são a fonte do template. Eles
foram criados como arquivos normais de projeto e depois parametrizados com
`.template.config/template.json`.

Novos tutoriais gerados por esse template já referenciam
`EFCore10.Shared`, carregam `appsettings.json` pelo helper compartilhado e
incluem uma connection string SQLite inicial baseada no `slug` informado.

Campos importantes do `template.json`:

- `shortName`: nome usado para instanciar o template, hoje `efcore10-tutorial`.
- `sourceName`: token de projeto/nome substituído por `--name`.
- `symbols`: parâmetros extras como `tutorialId`, `slug`, `title` e
  `className`.
- `fileRename`: renomeia `TutorialClass.cs` para o nome de classe informado.

Instale ou atualize todos os templates locais:

```bash
./scripts/dotnet-new-local.sh install ./templates --force
```

Liste os templates locais:

```bash
./scripts/dotnet-new-local.sh list
./scripts/dotnet-new-local.sh list efcore10
```

Crie apenas o projeto class library do tutorial:

```bash
./scripts/dotnet-new-local.sh efcore10-tutorial \
  --name EFCore10.Tutorials.Tutorial04 \
  --output src/EFCore10.Tutorials.Tutorial04 \
  --tutorialId 04 \
  --slug primitive-collections \
  --title "Primitive collections" \
  --className PrimitiveCollectionsTutorial
```

## Comando local para tutoriais

Para tutoriais, use o comando do repositório em vez de chamar o template
diretamente:

```bash
./scripts/new-tutorial.sh 04 primitive-collections "Primitive collections" PrimitiveCollectionsTutorial
```

`new-tutorial.sh` faz três coisas:

1. Instala ou atualiza `./templates` no hive local.
2. Cria o projeto de tutorial com `efcore10-tutorial`.
3. Registra o projeto gerado no solution folder `/src/` com `dotnet sln add` e
   adiciona a referência no app com `dotnet add reference`.

O terceiro passo é específico deste repositório. Um template comum de
`dotnet new` cria arquivos, mas este app só descobre DLLs de tutorial que são
referenciadas por `src/EFCore10.App/EFCore10.App.csproj`.

## Como criar outro template local

Use a mesma estrutura do tutorial da Microsoft, mas instale pelo wrapper local:

1. Crie uma pasta em `templates/<nome-do-template>/`.
2. Coloque nessa pasta os arquivos de projeto ou item que devem ser gerados.
3. Adicione `templates/<nome-do-template>/.template.config/template.json`.
4. Defina pelo menos `identity`, `name`, `shortName`, `sourceName` e `tags`.
5. Instale localmente:

   ```bash
   ./scripts/dotnet-new-local.sh install ./templates --force
   ```

6. Verifique localmente:

   ```bash
   ./scripts/dotnet-new-local.sh list <shortName-ou-palavra-chave>
   ./scripts/dotnet-new-local.sh <shortName> --dry-run --name Example
   ```

Se o template precisar de passos específicos do repositório, crie um script
pequeno em `scripts/` que chame `dotnet-new-local.sh` e depois execute os
comandos necessários. `scripts/new-tutorial.sh` é o exemplo a copiar.

## Referências

- Microsoft tutorial: https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-project-template
- `dotnet new install`: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-install
- `dotnet new list`: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-list
