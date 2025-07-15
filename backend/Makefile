# Makefile pour Entity Framework Core - Janus

PROJECT := src/Janus.Infrastructure/
STARTUP := src/Janus.API/

.PHONY: help migration-add migration-update migration-rollback migration-remove migration-list db-status

help:
	@echo "Commandes EF Core disponibles:"
	@echo "  make migration-add NAME=<nom>     - Créer une nouvelle migration"
	@echo "  make migration-update             - Appliquer les migrations"
	@echo "  make migration-rollback TO=<nom>  - Revenir à une migration"
	@echo "  make migration-remove             - Supprimer la dernière migration"
	@echo "  make migration-list               - Lister les migrations"
	@echo "  make db-status                    - Statut de la base de données"
	@echo ""
	@echo "Exemples:"
	@echo "  make migration-add NAME=AddUserProfile"
	@echo "  make migration-rollback TO=InitialCreate"

migration-add:
	@if [ -z "$(NAME)" ]; then \
		echo "❌ Error: NAME required. Usage: make migration-add NAME=<MigrationName>"; \
		exit 1; \
	fi
	@echo "🚀 Creating migration: $(NAME)"
	dotnet ef migrations add $(NAME) --project $(PROJECT) --startup-project $(STARTUP)

migration-update:
	@echo "🔄 Updating database..."
	dotnet ef database update --project $(PROJECT) --startup-project $(STARTUP)

migration-rollback:
	@if [ -z "$(TO)" ]; then \
		echo "❌ Error: TO required. Usage: make migration-rollback TO=<MigrationName>"; \
		exit 1; \
	fi
	@echo "⏮️  Rolling back to: $(TO)"
	dotnet ef database update $(TO) --project $(PROJECT) --startup-project $(STARTUP)

migration-remove:
	@echo "🗑️  Removing last migration..."
	dotnet ef migrations remove --project $(PROJECT) --startup-project $(STARTUP)

migration-list:
	@echo "📋 Listing migrations..."
	dotnet ef migrations list --project $(PROJECT) --startup-project $(STARTUP)

db-status:
	@echo "📊 Database and migrations status..."
	dotnet ef migrations list --project $(PROJECT) --startup-project $(STARTUP)

run:
	@echo "🚀 Running the application..."
	dotnet watch run --project $(STARTUP)
