# Author: Dwain
.PHONY: help setup up down run test test-sql migrate reset-db clean fresh db-check

# Default target
help:
	@printf "\033[1;32mDyadic - AVAILABLE COMMANDS\033[0m\n"
	@echo " make setup          FIRST-TIME LOCAL SETUP (DOCKER, SECRETS, MIGRATIONS)"
	@echo " make up             START THE SQL SERVER CONTAINER"
	@echo " make down           STOP THE SQL SERVER CONTAINER (KEEP DATA!)"
	@echo " make run            RUN THE WEB APP"
	@echo " make test           RUN UNIT + INMEMORY TESTS (NO DOCKER REQUIRED)"
	@echo " make test-sql       RUN SQL SERVER INTEGRATION TESTS (REQUIRES: make up)"
	@echo " make migrate        APPLY PENDING EF CORE MIGRATIONS"
	@echo " make reset-db       WIPE DATABASE AND RE-APPLY ALL MIGRATIONS (BE CAREFUL!)"
	@echo " make clean          REMOVE BUILD ARTIFACTS (bin/, obj/)"
	@echo " make fresh          FULL RESET: CLEAN + RESET-DB"
	@echo " make db-check       CHECK CONDITION OF DOCKER DATABASE"

setup:
	chmod +x ./setup.sh
	./setup.sh

up:
	docker compose up -d

down:
	docker compose down

run:
	dotnet run --project src/Dyadic.Web

test:
	dotnet test --filter "Category!=SqlServer"

test-sql:
	dotnet test --filter "Category=SqlServer"

migrate:
	dotnet ef database update -p src/Dyadic.Infrastructure -s src/Dyadic.Web

reset-db:
	docker compose down -v
	docker compose up -d
	@echo
	@printf "\033[1;32mWAITING FOR SQL SERVER TO READY...\033[0m\n"
	@echo
	@sleep 15
	$(MAKE) migrate

clean:
	find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +

fresh: clean reset-db
	dotnet build

db-check: ## Verify SQL Server is Accepting Connections
	@docker ps --filter "name=dyadic-sql" --filter "status=running" --quiet | grep -q . \
			|| { printf "\033[1;31m[fail]\033[0m  CONTAINER dyadic-sql IS NOT RUNNING. TRY: make up\n"; exit 1; }
	@SA_PASSWORD=$$(grep '^MSSQL_SA_PASSWORD=' .env | cut -d= -f2- | tr -d '\r'); \
	if docker exec dyadic-sql //opt/mssql-tools18/bin/sqlcmd \
		-S localhost -U sa -P "$$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then \
		printf "\033[1;32m[ok]\033[0m    SQL SERVER IS READY\n"; \
	else \
		printf "\033[1;31m[fail]\033[0m  SQL SERVER NOT RESPONDING... CHECK: docker compose logs db\n"; \
		exit 1; \
	fi 