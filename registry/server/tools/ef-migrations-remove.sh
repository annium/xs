#!/usr/bin/env bash
dotnet ef migrations remove \
    --startup-project src/Xs.Registry.Main \
    --project src/Xs.Registry.Db \
    --context Context \
    --no-build