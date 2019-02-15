#!/usr/bin/env bash
read -p "Migration name? " migration_name
dotnet ef database update $migration_name \
    --startup-project src/Xs.Registry.Main \
    --project src/Xs.Registry.Db \
    --context Context \
    --no-build