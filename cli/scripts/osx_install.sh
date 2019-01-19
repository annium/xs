#!/usr/bin/env bash

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))

root=/usr/local/share/xs
entry=/usr/local/bin/xs

echo "Compile"
rm -rf $root
dotnet publish -c release -r osx-x64 -o $root $dir/src/Xs.Cli.Main/

# prepare launcher
echo "Write launcher"
rm -f $entry
echo '#!/usr/bin/env bash' > $entry
echo $root'/Xs.Cli.Main $@' >> $entry
chmod +x $entry
