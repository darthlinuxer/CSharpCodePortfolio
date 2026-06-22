#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
hive="$repo_root/.dotnet-template-hive"

exec dotnet new --debug:custom-hive "$hive" "$@"
