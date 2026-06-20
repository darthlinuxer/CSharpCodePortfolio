#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'USAGE'
Usage:
  scripts/new-tutorial.sh <id> <slug> <title> [class-name]

Example:
  scripts/new-tutorial.sh 03 compiled-models "Compiled models" CompiledModelsTutorial
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
template_root="$repo_root/templates"
dotnet_new="$repo_root/scripts/dotnet-new-local.sh"
project_name="EFCore10.Tutorials.Tutorial${id}"
project_dir="$repo_root/src/$project_name"
project_file="$project_dir/$project_name.csproj"
app_project="$repo_root/src/EFCore10.App/EFCore10.App.csproj"
solution="$repo_root/EFCore10.slnx"

if [[ -e "$project_dir" ]]; then
  echo "Tutorial project already exists: $project_dir" >&2
  exit 1
fi

"$dotnet_new" install "$template_root" --force >/dev/null

"$dotnet_new" efcore10-tutorial \
  --name "$project_name" \
  --output "$project_dir" \
  --tutorialId "$id" \
  --slug "$slug" \
  --title "$title" \
  --className "$class_name"

dotnet sln "$solution" add "$project_file" --solution-folder /src
dotnet add "$app_project" reference "$project_file"

echo "Created and registered $project_name"
