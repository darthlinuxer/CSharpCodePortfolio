#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'USAGE'
Usage:
  scripts/new-tutorial.sh <id> <slug> <title> [class-name]

Example:
  scripts/new-tutorial.sh async-await async-await "Async/Await fundamentals" AsyncAwaitTutorial
USAGE
}

if [[ $# -lt 3 || $# -gt 4 ]]; then
  usage >&2
  exit 2
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
id="$1"
slug="$2"
title="$3"
class_name="${4:-Tutorial${id}}"
project_name="CSharpCodePortfolio.Tutorials.Tutorial${id}"
project_dir="$repo_root/src/$project_name"
project_file="$project_dir/$project_name.csproj"
template_root="$repo_root/templates"
dotnet_new="$repo_root/scripts/dotnet-new-local.sh"
app_project="$repo_root/src/CSharpCodePortfolio.App/CSharpCodePortfolio.App.csproj"
solution="$repo_root/CSharpCodePortfolio.slnx"

if [[ -e "$project_dir" ]]; then
  echo "Tutorial project already exists: $project_dir" >&2
  exit 1
fi

"$dotnet_new" install "$template_root" --force >/dev/null

"$dotnet_new" portfolio-tutorial \
  --name "$project_name" \
  --output "$project_dir" \
  --tutorialId "$id" \
  --slug "$slug" \
  --title "$title" \
  --className "$class_name"

dotnet sln "$solution" add "$project_file" --solution-folder /src
dotnet add "$app_project" reference "$project_file"

echo "Created and registered $project_name"
