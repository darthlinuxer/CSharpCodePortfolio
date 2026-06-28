#!/usr/bin/env bash
set -euo pipefail

if [[ $# -eq 0 ]]; then
    dotnet run --project src/CSharpCodePortfolio.App -- menu
elif [[ $# -eq 1 && "$1" =~ ^[0-9]+$ ]]; then
    tutorial_id="$1"

    if [[ ${#tutorial_id} -eq 1 ]]; then
        tutorial_id="0$tutorial_id"
    fi

    dotnet run --project src/CSharpCodePortfolio.App -- run "$tutorial_id"
else
    echo "Usage: $0 [tutorial-number]" >&2
    exit 2
fi
