$dir = (Get-Item $PSScriptRoot).Parent.FullName

$root = Join-Path $HOME Projects lib xs
New-Item -Path (Join-Path $HOME Projects) -Name bin -ItemType Directory -Force | Out-Null

Write-Output "Compile."
if (Test-Path $root) { Remove-Item -Recurse -Force $root }
dotnet publish -c release -r win-x64 -o $root $dir/src/Xs.Cli.Main/

# prepare launcher
$entry = Join-Path $HOME Projects bin xs.bat
Write-Output "Write launcher."
if (Test-Path $entry) { Remove-Item -Recurse -Force $entry }
Write-Output "run $root/Xs.Cli.Main $@" > $entry

# prepare relaxed launcher
$relaxed = Join-Path $HOME Projects bin ass.bat
Write-Output "Write relaxed launcher."
if (Test-Path $relaxed) { Remove-Item -Recurse -Force $relaxed }
Write-Output $root'/Xs.Cli.Main $@ --skip-checks' > $relaxed

# prepare simple launcher
$simple = Join-Path $HOME Projects bin xss.bat
Write-Output "Write simple launcher."
if (Test-Path $simple) { Remove-Item -Recurse -Force $simple }
Write-Output $root'/Xs.Cli.Main $@ --ignore-consistency --skip-checks' > $simple