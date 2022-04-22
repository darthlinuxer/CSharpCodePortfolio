#!/bin/ash
set -euo pipefail

if [ -f /run/secrets/API_KESTREL_PASSWORD ]; then
   export RAVEN_Security_Certificate_Password=$(cat $RAVEN_Security_Certificate_Password_FILE)
fi

echo Environment Variables
printenv

