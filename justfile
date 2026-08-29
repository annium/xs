set shell := ["bash", "-cu"]
set positional-arguments
# lib.just is copied in by the umbrella repo's `just copy-ci`; recipes redefined below
# override the shared ones.
set allow-duplicate-recipes := true

import 'lib.just'

project_name := "pkg"
tag_prefix := "registry.annium.com/" + project_name
tfm := "net10.0"
bin_release := "bin/Release/" + tfm

# base - only what differs from the shared recipes

update:
    @echo "=== $0 ==="
    dotnet tool list --format json | jq -r '.data[] | "\(.packageId)"' | xargs -I% dotnet tool install %
    dotnet tool run xs update all dotnet -sc -ic

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

# ci - the shared recipes run a docs step this repo has no tooling for

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
    just clean
    just build
    just test

ci-release apiKey repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-release ==="
    just setup
    just format
    just ensure-no-changes
    just ci-set-package-version
    just clean
    just build
    just test
    just pack
    just publish "$1"
    just ci-push-tag "$2" "$3"
    echo "Release complete"

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
