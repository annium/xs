PROJECT_NAME := pkg
TAG_PREFIX := registry.annium.com/$(PROJECT_NAME)
TFM := net7.0
BIN_DEBUG := bin/Debug/$(TFM)

configure:
	@# host
	$(call copy,shared,application.yml email.yml,run/server/configuration server/src/Server.Host/configuration)
	$(call copy,docker,db.yml,run/server/configuration)
	$(call copy,local,db.yml,server/src/Server.Host/configuration)
	$(call copy,shared,private.key public.key,run/server/keys server/src/Server.Host/keys)

	@# db
	$(call copy,docker,db.env,run/db)

	@# server tests
	$(call copy,shared,private.key public.key,server/test/Server.IntegrationTests/keys)

	@# demo host
	$(call copy,shared,private.key public.key,server/test/Server.DemoHost/keys)

	@# core tests
	$(call copy,shared,private.key public.key,lib/test/Annium.Id.Core.Tests/keys)

deconfigure:
	rm -rf run
	$(call clean,/configuration/ /keys/)


db-drop:
	docker-compose rm -vfs db
	docker volume rm -f id_db
	docker-compose up -d db

link:
	@./cli/scripts/link.js ../backend

unlink:
	@./cli/scripts/unlink.js ../backend

.PHONY: $(MAKECMDGOALS)