db-up main-up dotnet-up node-up:
	$(call up,$(subst -up,,$@))

db-down main-down dotnet-down node-down:
	$(call down,$(subst -down,,$@))

db-log main-log dotnet-log node-log:
	$(call log,$(subst -log,,$@))


db-drop db-update migrations-add migrations-list migrations-remove:
	tools/ef-$@.sh



define up
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	$(call net-up)
	@rm -rf src/packages
	@cp -rf /projects/annium/packages src/packages
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p registry $(files) up --build -d --force-recreate
	@rm -rf src/packages
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