#!/usr/bin/env bash

dotnet restore --force --no-cache && dotnet build
dotnet test

echo "Pulled new changes and ran all tests."
