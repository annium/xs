#!/usr/bin/env sh

context=$1
assembly=$2
startup_assembly=$3

dotnet exec \
	--depsfile $startup_assembly.deps.json \
	--runtimeconfig $startup_assembly.runtimeconfig.json \
	ef.dll \
	database update \
	--context $context \
	--assembly $assembly.dll \
	--startup-assembly $startup_assembly.dll