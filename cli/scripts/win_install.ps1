$dir = Join-Path (Get-Item $PSScriptRoot).Parent.FullName src Xx

Write-Output "Compile."
dotnet pack $dir --configuration release --output . -p:DefineConstants=\"LOG_CORE\;LOG_DEBUG\;LOG_TRACE\"

if ( (dotnet tool list -g | Select-Object -skip 2 | Measure-Object).Count -eq 1 ) {
    Write-Output "Uninstall."
    dotnet tool uninstall -g xx
}

Write-Output "Install."
dotnet tool install -g xx --add-source .

Write-Output "Cleanup."
Get-ChildItem . -File -Filter '*.nupkg' | Remove-Item
