PROJECT_NAME := registry
TAG_PREFIX := brutallord.ddns.net:5000/$(PROJECT_NAME)

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
	$(call publish,main,server/src,Xs.Registry.Main/Dockerfile)
	@rm -f server/src/ef.dll

publish-dotnet:
	$(call publish,dotnet,server/src,Xs.Registry.Dotnet/Dockerfile)

publish-node:
	$(call publish,node,server/src,Xs.Registry.Node/Dockerfile)

publish-site:
	$(call publish,site,site,Dockerfile)


define publish
	@$(eval image := $(1))
	@$(eval context := $(2))
	@$(eval dockerfile := $(3))
	$(if $(LOCAL_BUILD),$(call copy-packages))
	@docker build -t $(TAG_PREFIX)/$(image) -f $(context)/$(dockerfile) $(context)
	$(if $(LOCAL_BUILD),$(call delete-packages))
	@docker push $(TAG_PREFIX)/$(image)
endef

define up
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	$(call net-up)
	$(if $(LOCAL_BUILD),$(call copy-packages))
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p $(PROJECT_NAME) $(files) up --build -d --force-recreate
	$(if $(LOCAL_BUILD),$(call delete-packages))
endef

define down
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p $(PROJECT_NAME) $(files) down
	$(call net-down)
endef

define copy-packages
	@rm -rf server/src/packages
	@cp -rf /annium/packages server/src/packages
endef

define delete-packages
	@rm -rf server/src/packages
endef

define log
	@$(eval component := $(1))
	@docker logs -f $(PROJECT_NAME)_$(component)
endef

define net-up
	@if [[ $$(docker network ls -q -f "name=$(PROJECT_NAME)" | wc -l) -eq 0 ]]; then \
		docker network create --attachable --driver bridge --scope local $(PROJECT_NAME); \
	fi
endef

define net-down
	@if [[ $$(docker network ls -q -f "name=$(PROJECT_NAME)" | wc -l) -gt 0 ]]; then \
		if [[ $$(docker network inspect -f "{{len .Containers}}" $(PROJECT_NAME)) -eq 0 ]]; then \
			docker network rm $(PROJECT_NAME); \
		fi \
	fi
endef

.PHONY: $(MAKECMDGOALS)