#!/bin/sh

if [ "$ASPNETCORE_ENVIRONMENT" = "Production" ]; then
  echo "Running production build..."
  cd /app/publish
  dotnet RSPWebAPI.dll
else
  echo "Running development server..."
  cd /app/src
  dotnet watch run --project RSPWebAPI.csproj --urls=http://0.0.0.0:4000
fi
