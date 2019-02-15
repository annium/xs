#!/usr/bin/env bash
dotnet ef migrations list \
    --startup-project src/Xs.Registry.Main \
    --project src/Xs.Registry.Db \
    --context Context \
    --no-build