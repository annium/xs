TAG_PREFIX := brutallord.ddns.net:5000/registry

db-up main-up dotnet-up node-up site-up:
	$(call up,$(subst -up,,$@))

db-down main-down dotnet-down node-down site-down:
	$(call down,$(subst -down,,$@))

db-log main-log dotnet-log node-log site-log:
	$(call log,$(subst -log,,$@))


db-drop db-update migrations-add migrations-list migrations-remove:
	cd server && tools/ef-$@.sh

publish-main:
	@cp $$(find $$(dirname $$(realpath $$(which dotnet)))/sdk -type f -name ef.dll | grep netcoreapp2.2) server/src
	@cp server/tools/ef.sh server/src
	$(call publish,main,server/src,Xs.Registry.Main/Dockerfile)
	@rm -f server/src/ef.dll
	@rm -f server/src/ef.sh

publish-dotnet:
	$(call publish,dotnet,server/src,Xs.Registry.Dotnet/Dockerfile)

publish-node:
	$(call publish,node,server/src,Xs.Registry.Node/Dockerfile)

publish-site:
	$(call publish,site,site/src,site/Dockerfile)

define publish
	@$(eval image := $(1))
	@$(eval context := $(2))
	@$(eval dockerfile := $(3))
	@rm -rf server/src/packages
	@cp -rf /projects/annium/packages server/src/packages
	@docker build -t $(TAG_PREFIX)/$(image) -f $(context)/$(dockerfile) $(context)
	@rm -rf server/src/packages
	@docker push $(TAG_PREFIX)/$(image)
endef

define up
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	$(call net-up)
	@rm -rf server/src/packages
	@cp -rf /projects/annium/packages server/src/packages
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p registry $(files) up --build -d --force-recreate
	@rm -rf server/src/packages
endef

define down
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p registry $(files) down
	$(call net-down)
endef

define log
	@$(eval component := $(1))
	@docker logs -f registry_$(component)
endef

define net-up
	@if [[ $$(docker network ls -q -f "name=registry" | wc -l) -eq 0 ]]; then \
		docker network create --attachable --driver bridge --scope local registry; \
	fi
endef

define net-down
	@if [[ $$(docker network ls -q -f "name=registry" | wc -l) -gt 0 ]]; then \
		if [[ $$(docker network inspect -f "{{len .Containers}}" registry) -eq 0 ]]; then \
			docker network rm registry; \
		fi \
	fi
endef

.PHONY: $(MAKECMDGOALS)