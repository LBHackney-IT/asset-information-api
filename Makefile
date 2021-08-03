.PHONY: setup
setup:
	docker-compose build

.PHONY: build
build:
	docker-compose build asset-information-api

.PHONY: serve
serve:
	docker-compose build asset-information-api && docker-compose up asset-information-api

.PHONY: shell
shell:
	docker-compose run asset-information-api bash

.PHONY: test
test:
	docker-compose up dynamodb-database & docker-compose build asset-information-api-test && docker-compose up asset-information-api-test

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter ancestor=dynamodb-database -a)
	-docker rm $$(docker ps -q --filter ancestor=dynamodb-database -a)
	docker rmi dynamodb-database
	docker-compose up -d dynamodb-database
