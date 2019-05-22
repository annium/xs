$dir = (Get-Item $PSScriptRoot).Parent.FullName

$root = Join-Path $HOME Projects lib xs
$entry = Join-Path $HOME Projects bin xs.bat

echo "Compile."
rm -r $root
dotnet publish -c release -r win-x64 -o $root $dir/src/Xs.Cli.Main/

# prepare launcher
echo "Write launcher."
rm -f $entry
mkdir -p (Split-Path  $entry)
echo "run $root/Xs.Cli.Main $@" > $entry

# prepare relaxed launcher
$relaxed = Join-Path $HOME Projects bin ass.bat
echo "Write relaxed launcher."
rm -f $relaxed
mkdir -p (Split-Path  $relaxed)
echo $root'/Xs.Cli.Main $@ --skip-checks' > $relaxed

# prepare simple launcher
$simple = Join-Path $HOME Projects bin xss.bat
echo "Write simple launcher."
rm -f $simple
mkdir -p (Split-Path  $simple)
echo $root'/Xs.Cli.Main $@ --ignore-consistency --skip-checks' > $simple
