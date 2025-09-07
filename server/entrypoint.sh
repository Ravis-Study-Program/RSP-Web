#!/bin/sh

if [ "$ASPNETCORE_ENVIRONMENT" = "Production" ]; then
  echo "Running production build..."
  cd /app
  exec dotnet RSPWebAPI.dll
else
  echo "Running development build..."
  cd /app/src
  exec dotnet watch run --urls "http://0.0.0.0:4000"
fi
