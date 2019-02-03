#!/usr/bin/env bash
dotnet ef migrations list \
    --startup-project src/Xs.Registry.$1 \
    --project src/Xs.Registry.$2 \
    --context $3DbContext \
    --no-build