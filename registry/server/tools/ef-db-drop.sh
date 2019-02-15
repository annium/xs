#!/usr/bin/env bash
dotnet ef database drop -f \
    --startup-project src/Xs.Registry.Main \
    --project src/Xs.Registry.Db \
    --context Context \
    --no-build