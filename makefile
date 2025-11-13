PROJECT_NAME := pkg
TAG_PREFIX := registry.annium.com/$(PROJECT_NAME)
TFM := net9.0
BIN_RELEASE := bin/Release/$(TFM)

setup:
	$(call header)
	dotnet tool restore

format:
	$(call header)
	dotnet tool run csharpier format . --config-path $(shell pwd)/.editorconfig
	dotnet tool run xs format -sc -ic

format-full: format
	$(call header)
	dotnet format style
	dotnet format analyzers

ensure-no-changes:
	$(call header)
	@if [[ -n "$$(git status --porcelain)" ]]; then \
		echo "Changes detected:"; \
		git status; \
		git --no-pager diff --no-color --exit-code; \
	fi

update:
	$(call header)
	dotnet tool list --format json | jq -r '.data[] | "\(.packageId)"' | xargs -I% dotnet tool install %
	dotnet tool run xs update all dotnet -sc -ic

clean:
	$(call header)
	dotnet tool run xs clean -sc -ic
	find . -type f -name '*.nupkg' | xargs -I% rm %

build:
	$(call header)
	$(call get-package-version)
	dotnet build -c Release --nologo -v q -p:PackageVersion=$(packageVersion)

test:
	$(call header)
	dotnet test -c Release --no-build --nologo --logger "trx;LogFilePrefix=test-results.trx"

pack:
	$(call header)
	$(call get-package-version)
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg -p:PackageVersion=$(packageVersion)

publish:
	$(call header)
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(apiKey)
	find . -type f -name '*.nupkg' | xargs -I% rm %

install-cli:
	$(call header)
	./cli/scripts/nix_install.sh

uninstall-cli:
	$(call header)
	./cli/scripts/nix_uninstall.sh

configure:
	$(call header)
	@# host
	$(call copy,shared,main.yml,run/server/configuration server/src/Annium.Xs.Server.Host/configuration)
	$(call copy,docker,db.yml,run/server/configuration)
	$(call copy,local,db.yml,server/src/Annium.Xs.Server.Host/configuration)

	@# db
	$(call copy,docker,db.env,run/db)

deconfigure:
	$(call header)
	rm -rf run
	$(call clean,/configuration/ /keys/)


run:
	$(call header)
	cd server/src/Annium.Xs.Server.Host && ./$(BIN_RELEASE)/Annium.Xs.Server.Host

publish-all: publish-server

publish-server:
	$(call header)
	$(call publish,server,.,server/src/Annium.Xs.Server.Host/app.dockerfile)

publish-local: publish-server-local

publish-server-local:
	$(call header)
	$(call publish,server,..,xs/server/src/Annium.Xs.Server.Host/app.local.dockerfile)

db-drop:
	$(call header)
	docker-compose rm -vfs db
	docker volume rm -f xs_db
	docker-compose up -d db

link:
	$(call header)
	@./cli/scripts/link.js ../backend

unlink:
	$(call header)
	@./cli/scripts/unlink.js ../backend

# CI
ci-merge-request-short:
	$(call header)
	make setup
	make format
	make ensure-no-changes
	make clean
	make build

ci-merge-request-full:
	$(call header)
	make setup
	make format
	make ensure-no-changes
	make docs-lint
	make clean
	make build
	make test
	make docs-build

ci-release:
	$(call header)
	make setup
# 	make format
	make ensure-no-changes
	make ci-set-package-version
	make clean
	make build
	make pack
	make docs-build
	make publish apiKey=$(shell cat .xs.credentials)
	make ci-push-tag repository=$(repository) githubToken=$(githubToken)
	echo "Release complete"

ci-set-package-version:
	$(call header)
#	git config user.name "it"
#	git config user.email "it@annium.com"
	dotnet tool run versioning set-version -v $(shell cat version)

ci-push-tag:
	$(call header)
	$(call get-package-version)
#	git remote set-url origin https://x-access-token:$(githubToken)@github.com/$(repository).git
	git push origin v$(packageVersion)


define publish
	@$(eval image := $(1))
	@$(eval context := $(2))
	@$(eval dockerfile := $(3))
	docker build -t $(TAG_PREFIX)/$(image) -f $(context)/$(dockerfile) $(context)
	docker push $(TAG_PREFIX)/$(image)
endef

define copy
	$(foreach dir,$(3),mkdir -p $(dir);$(foreach file,$(2),cp cfg/$(1)/$(file) $(dir);))
endef

define clean
	$(foreach pattern,$(1),git ls-files --others . | grep $(pattern) | xargs rm -f;)
endef


define header
	@echo "=== $@ ==="
endef

define get-package-version
	$(eval packageVersion := $(shell dotnet tool run versioning get-version -v $(shell cat version)))
endef


.PHONY: $(MAKECMDGOALS)
