set shell := ["bash", "-cu"]
set positional-arguments

project_name := "pkg"
tag_prefix := "registry.annium.com/" + project_name
tfm := "net9.0"
bin_release := "bin/Release/" + tfm

[private]
default:
    @just --list

# base

setup:
    @echo "=== $0 ==="
    dotnet tool restore

format:
    @echo "=== $0 ==="
    dotnet tool run csharpier format . --config-path $(pwd)/.editorconfig
    dotnet tool run xs format -sc -ic

format-full: format
    @echo "=== $0 ==="
    dotnet format style
    dotnet format analyzers

ensure-no-changes:
    #!/usr/bin/env bash
    set -e
    echo "=== ensure-no-changes ==="
    if [[ -n "$(git status --porcelain)" ]]; then
        echo "Changes detected:"
        git status
        git --no-pager diff --no-color --exit-code
    fi

update:
    @echo "=== $0 ==="
    dotnet tool list --format json | jq -r '.data[] | "\(.packageId)"' | xargs -I% dotnet tool install %
    dotnet tool run xs update all dotnet -sc -ic

clean:
    @echo "=== $0 ==="
    dotnet tool run xs clean -sc -ic
    find . -type f -name '*.nupkg' | xargs -I% rm %

build:
    #!/usr/bin/env bash
    set -e
    echo "=== build ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet build -c Release --nologo -v q -p:PackageVersion=$packageVersion

test:
    @echo "=== $0 ==="
    dotnet test -c Release --no-build --nologo --logger "trx;LogFilePrefix=test-results.trx"

pack:
    #!/usr/bin/env bash
    set -e
    echo "=== pack ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg -p:PackageVersion=$packageVersion

publish apiKey:
    @echo "=== $0 ==="
    dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key "$1"
    find . -type f -name '*.nupkg' | xargs -I% rm %

# cli

install-cli:
    @echo "=== $0 ==="
    ./cli/scripts/nix_install.sh

uninstall-cli:
    @echo "=== $0 ==="
    ./cli/scripts/nix_uninstall.sh

# configuration

configure:
    #!/usr/bin/env bash
    set -e
    echo "=== configure ==="
    # host
    just _copy shared main.yml "run/server/configuration server/src/Annium.Xs.Server.Host/configuration"
    just _copy docker db.yml run/server/configuration
    just _copy local db.yml server/src/Annium.Xs.Server.Host/configuration
    # db
    just _copy docker db.env run/db

deconfigure:
    #!/usr/bin/env bash
    set -e
    echo "=== deconfigure ==="
    rm -rf run
    for pattern in /configuration/ /keys/; do
        git ls-files --others . | grep "$pattern" | xargs -r rm -f
    done

# run

run:
    @echo "=== $0 ==="
    cd server/src/Annium.Xs.Server.Host && ./{{bin_release}}/Annium.Xs.Server.Host

# publish

publish-all: publish-server

publish-server:
    @echo "=== $0 ==="
    @just _publish server . server/src/Annium.Xs.Server.Host/app.dockerfile

publish-local: publish-server-local

publish-server-local:
    @echo "=== $0 ==="
    @just _publish server .. xs/server/src/Annium.Xs.Server.Host/app.local.dockerfile

# infra

db-drop:
    @echo "=== $0 ==="
    docker-compose rm -vfs db
    docker volume rm -f xs_db
    docker-compose up -d db

# cli link

link:
    @echo "=== $0 ==="
    @./cli/scripts/link.js ../backend

unlink:
    @echo "=== $0 ==="
    @./cli/scripts/unlink.js ../backend

# ci

ci-merge-request-short:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-merge-request-short ==="
    just setup
    just format
    just ensure-no-changes
    just clean
    just build

ci-merge-request-full:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-merge-request-full ==="
    just setup
    just format
    just ensure-no-changes
    just docs-lint
    just clean
    just build
    just test
    just docs-build

ci-release repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-release ==="
    just setup
    just format
    just ensure-no-changes
    just ci-set-package-version
    just clean
    just build
    just pack
    just docs-build
    just publish "$(cat .xs.credentials)"
    just ci-push-tag "$1" "$2"
    echo "Release complete"

ci-set-package-version:
    @echo "=== $0 ==="
    dotnet tool run versioning set-version -v $(cat version)

ci-push-tag repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-push-tag ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    git push origin v$packageVersion

# private helpers

_publish image context dockerfile:
    docker build -t {{tag_prefix}}/{{image}} -f {{context}}/{{dockerfile}} {{context}}
    docker push {{tag_prefix}}/{{image}}

_copy source files dests:
    #!/usr/bin/env bash
    set -e
    for dir in {{dests}}; do
        mkdir -p "$dir"
        for file in {{files}}; do
            cp "cfg/{{source}}/$file" "$dir"
        done
    done
