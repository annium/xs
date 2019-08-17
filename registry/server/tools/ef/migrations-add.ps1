param(
    [Parameter(Mandatory = $true)][string]$startup,
    [Parameter(Mandatory = $true)][string]$project,
    [Parameter(Mandatory = $true)][string]$context
)

$env:ASPNETCORE_ENVIRONMENT = 'LocalBase'

$migrationName = Read-Host -Prompt "Migration name?"
dotnet ef migrations add $migrationName `
    --startup-project $startup `
    --project $project `
    --context $context `
    --no-build