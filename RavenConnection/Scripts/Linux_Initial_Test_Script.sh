#!/bin/ash
set -euo pipefail

if [ -f /run/secrets/API_KESTREL_PASSWORD ]; then
   export API_KESTREL_PASSWORD=$(cat /run/secrets/API_KESTREL_PASSWORD)
fi

if [ -f /run/secrets/RAVEN_SERVER_PASSWORD ]; then
   export RAVEN_SERVER_PASSWORD=$(cat /run/secrets/RAVEN_SERVER_PASSWORD)
fi

echo Environment Variables
printenv
echo Scripts
ls -la /Scripts
echo Certificate
ls -la /$API_CERTIFICATE_PATH

