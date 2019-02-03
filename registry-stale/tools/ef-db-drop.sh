#!/usr/bin/env bash
dotnet ef database drop -f \
    --startup-project src/Xs.Registry.$1 \
    --project src/Xs.Registry.$2 \
    --context $3 \
    --no-build