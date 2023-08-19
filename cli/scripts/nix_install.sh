#!/usr/bin/env bash
set -e

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))/src/Xs

echo "Compile."
dotnet pack $dir --configuration release --output . -p:DefineConstants=\"LOG_CORE\;LOG_DEBUG\;LOG_TRACE\"

if [ $(dotnet tool list -g | tail -n +3 | grep xs | wc -l) -eq 1 ]; then
    echo "Uninstall."
    dotnet tool uninstall -g xs
fi

echo "Install."
dotnet tool install -g xs --add-source .

echo "Cleanup."
find . -type f -name '*.nupkg' | xargs rm -f
