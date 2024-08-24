#!/usr/bin/env bash
set -e

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))/src/Xx

echo "Compile."
dotnet pack $dir --configuration release --output . -p:DefineConstants=\"LOG_CORE\;LOG_DEBUG\;LOG_TRACE\"

if [ $(dotnet tool list -g | tail -n +3 | grep xx | wc -l) -eq 1 ]; then
    echo "Uninstall."
    dotnet tool uninstall -g xx
fi

echo "Install."
dotnet tool install -g xx --add-source .

echo "Cleanup."
find . -type f -name '*.nupkg' | xargs rm -f
