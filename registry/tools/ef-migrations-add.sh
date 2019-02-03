#!/usr/bin/env bash
read -p "Migration name? " migration_name
dotnet ef migrations add $migration_name \
    --startup-project src/Xs.Registry.$1 \
    --project src/Xs.Registry.$2 \
    --context $3DbContext \
    --no-build