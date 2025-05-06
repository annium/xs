PROJECT_NAME := pkg
TAG_PREFIX := registry.annium.com/$(PROJECT_NAME)
TFM := net9.0
BIN_DEBUG := bin/Debug/$(TFM)

format:
	xx format -sc -ic
	dotnet csharpier .

setup:
	xx remote restore -user $(user) -password $(pass)
	dotnet tool restore

update:
	xx update all dotnet -sc -ic

clean:
	xx clean -sc -ic

buildNumber?=0
build:
	dotnet build -c Release --nologo -v q -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo -v q

pack:
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg

publish:
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(shell cat .xx.credentials)
	find . -type f -name '*.nupkg' | xargs rm

install-cli:
	./cli/scripts/nix_install.sh

uninstall-cli:
	./cli/scripts/nix_uninstall.sh

configure:
	@# host
	$(call copy,shared,main.yml,run/server/configuration server/src/Server.Host/configuration)
	$(call copy,docker,db.yml,run/server/configuration)
	$(call copy,local,db.yml,server/src/Server.Host/configuration)

	@# db
	$(call copy,docker,db.env,run/db)

deconfigure:
	rm -rf run
	$(call clean,/configuration/ /keys/)


run:
	cd server/src/Server.Host && ./bin/Debug/$(TFM)/Server.Host

publish-all: publish-server

publish-server:
	$(call publish,server,.,server/src/Server.Host/app.dockerfile)

publish-local: publish-server-local

publish-server-local:
	$(shell,find .. -type f -name nuget.config | xargs rm)
	$(call publish,server,..,xx/server/src/Server.Host/app.local.dockerfile)

db-drop:
	docker-compose rm -vfs db
	docker volume rm -f xx_db
	docker-compose up -d db

link:
	@./cli/scripts/link.js ../backend

unlink:
	@./cli/scripts/unlink.js ../backend

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

.PHONY: $(MAKECMDGOALS)