#!/bin/bash

set -euxo pipefail 
#-e	Exit immediately if a command exits with a non-zero status.
#-o pipefail	Set return code to the status of the last process in a pipe.
#-u	Bail if an undefined variable gets accessed.
#-x	Trace almost every command.

if [ -f /run/secrets/API_KESTREL_PASSWORD ]; then
   echo Creating environment variable from secrets!
   export ASPNETCORE_Kestrel__Certificates__Default__Password=$(cat /run/secrets/API_KESTREL_PASSWORD)
fi

if [ /app/https ]; then
   echo Directory /app/https exists!
   ls -la /app/https
fi

echo "Environment Variables"
printenv
cd /app
dotnet RavenConnection.dll
