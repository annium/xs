#!/usr/bin/env bash
dotnet ef migrations remove \
    --startup-project src/Xs.Registry.$1 \
    --project src/Xs.Registry.$2 \
    --context $3 \
    --no-build