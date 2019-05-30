$dir = (Get-Item $PSScriptRoot).Parent.FullName

$root = Join-Path $HOME Documents Projects lib xs
New-Item -Path (Join-Path $HOME Documents Projects) -Name bin -ItemType Directory -Force | Out-Null

Write-Output "Compile."
if (Test-Path $root) { Remove-Item -Recurse -Force $root }
dotnet publish -c release -r win-x64 -o $root $dir/src/Xs.Cli.Main/

$nl = [System.Environment]::NewLine
# prepare launcher
$entry = Join-Path $HOME Documents Projects bin xs.bat
Write-Output "Write launcher."
if (Test-Path $entry) { Remove-Item -Recurse -Force $entry }
Set-Content -Path $entry -Value "@echo off $nl $root\Xs.Cli.Main.exe %*"

# prepare relaxed launcher
$relaxed = Join-Path $HOME Documents Projects bin ass.bat
Write-Output "Write relaxed launcher."
if (Test-Path $relaxed) { Remove-Item -Recurse -Force $relaxed }
Set-Content -Path $relaxed -Value "@echo off $nl $root\Xs.Cli.Main.exe %* --skip-checks"

# prepare simple launcher
$simple = Join-Path $HOME Documents Projects bin xss.bat
Write-Output "Write simple launcher."
if (Test-Path $simple) { Remove-Item -Recurse -Force $simple }
Set-Content -Path $simple -Value "@echo off $nl $root\Xs.Cli.Main.exe %* --skip-checks --ignore-consistency"