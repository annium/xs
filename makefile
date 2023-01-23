PROJECT_NAME := pkg
TAG_PREFIX := registry.annium.com/$(PROJECT_NAME)
TFM := net7.0
BIN_DEBUG := bin/Debug/$(TFM)

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


db-drop:
	docker-compose rm -vfs db
	docker volume rm -f xs_db
	docker-compose up -d db

link:
	@./cli/scripts/link.js ../backend

unlink:
	@./cli/scripts/unlink.js ../backend

define publish
	@$(eval image := $(1))
	@$(eval context := $(2))
	@$(eval dockerfile := $(3))
	@docker build -t $(TAG_PREFIX)/$(image) -f $(context)/$(dockerfile) $(context)
	@docker push $(TAG_PREFIX)/$(image)
endef

define copy
	$(foreach dir,$(3),mkdir -p $(dir);$(foreach file,$(2),cp cfg/$(1)/$(file) $(dir);))
endef

define clean
	$(foreach pattern,$(1),git ls-files --others . | grep $(pattern) | xargs rm -f;)
endef

.PHONY: $(MAKECMDGOALS)