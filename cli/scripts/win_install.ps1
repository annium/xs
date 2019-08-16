$dir = Join-Path (Get-Item $PSScriptRoot).Parent.FullName src Xs

Write-Output "Compile."
dotnet pack --configuration release --output . $dir

if ( (dotnet tool list -g | Select-Object -skip 2 | Measure-Object).Count -eq 1 ) {
    Write-Output "Uninstall."
    dotnet tool uninstall -g xs
}

Write-Output "Install."
dotnet tool install -g xs --add-source $dir

Write-Output "Cleanup."
Get-ChildItem $dir -File -Filter '*.nupkg' | Remove-Item
