param(
    [Parameter(Mandatory = $true)][string]$startup,
    [Parameter(Mandatory = $true)][string]$project,
    [Parameter(Mandatory = $true)][string]$context
)

$env:ASPNETCORE_ENVIRONMENT = 'LocalBase'

dotnet ef database drop -f `
    --startup-project $startup `
    --project $project `
    --context $context `
    --no-build