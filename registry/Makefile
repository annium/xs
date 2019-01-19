db-up:
	$(call up,db db_tool)
db-down:
	$(call down,db db_tool)
db-log:
	$(call log,db)

shared-up:
	$(call up,shared)
shared-down:
	$(call down,shared)
shared-log:
	$(call log,shared)

dotnet-up:
	$(call up,dotnet)
dotnet-down:
	$(call down,dotnet)
dotnet-log:	
	$(call log,dotnet)

node-up:
	$(call up,node)
node-down:
	$(call down,node)
node-log:
	$(call log,node)


define up
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	$(call net-up)
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p registry $(files) up --build -d --force-recreate
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