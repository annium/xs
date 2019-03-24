#!/usr/bin/env bash

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))

root=/usr/local/share/xs
entry=/usr/local/bin/xs

echo "Compile."
rm -rf $root
dotnet publish -c release -r osx-x64 -o $root $dir/src/Xs.Cli.Main/

# prepare launcher
echo "Write launcher."
rm -f $entry
echo '#!/usr/bin/env sh' > $entry
echo $root'/Xs.Cli.Main $@' >> $entry
chmod +x $entry

# prepare relaxed launcher
relaxed=/usr/local/bin/ass
echo "Write relaxed launcher."
rm -f $relaxed
echo '#!/usr/bin/env sh' > $relaxed
echo $root'/Xs.Cli.Main $@ --skip-checks' >> $relaxed
chmod +x $relaxed

# prepare simple launcher
simple=/usr/local/bin/xss
echo "Write simple launcher."
rm -f $simple
echo '#!/usr/bin/env sh' > $simple
echo $root'/Xs.Cli.Main $@ --ignore-consistency --skip-checks' >> $simple
chmod +x $simple
