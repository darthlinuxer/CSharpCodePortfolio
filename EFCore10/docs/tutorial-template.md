# EFCore10 Tutorial Template

This workspace keeps its tutorial template out of the global `dotnet new` template hive.

See the root `README.md` for the full process used to create local templates and
local `dotnet new` wrappers.

Create a tutorial project and register it in the solution and app:

```bash
./scripts/new-tutorial.sh 04 primitive-collections "Primitive collections"
```

You can pass a class name as the fourth argument when the default `Tutorial<id>`
class name is not descriptive enough:

```bash
./scripts/new-tutorial.sh 04 primitive-collections "Primitive collections" PrimitiveCollectionsTutorial
```

The script installs or refreshes the template in the workspace-local hive before
creating the project. No separate install step is required.

Generated tutorials reference `EFCore10.Shared`, include an `appsettings.json`
file, and start with the shared configuration/SQLite connection-string helpers
already wired into `RunAsync`.

To inspect the local template catalog manually:

```bash
./scripts/dotnet-new-local.sh list efcore10
```

To instantiate only the tutorial class library without registering it in the app:

```bash
./scripts/dotnet-new-local.sh efcore10-tutorial \
  --name EFCore10.Tutorials.Tutorial04 \
  --output src/EFCore10.Tutorials.Tutorial04 \
  --tutorialId 04 \
  --slug primitive-collections \
  --title "Primitive collections" \
  --className PrimitiveCollectionsTutorial
```

The generated project is discoverable by the menu after it is referenced by
`src/EFCore10.App/EFCore10.App.csproj`, because the app discovers tutorial DLLs
from its output directory.

`scripts/new-tutorial.sh` also registers the generated project under the `/src/`
solution folder so the `.slnx` stays visually consistent.

Generated tutorial classes implement `ITutorial`, so the compiler verifies the
required `RunAsync(CancellationToken)` method.
