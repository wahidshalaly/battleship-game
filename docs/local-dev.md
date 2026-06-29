# Local Development Guide

This guide covers running the Battleship Game locally with **.NET Aspire**. Aspire is the
single supported local-dev path — it orchestrates PostgreSQL, applies database migrations, and
starts the Web API for you. There is no Docker Compose flow and no in-memory persistence mode.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A container runtime (Docker Desktop or Podman) — Aspire uses it to run the PostgreSQL and
  pgAdmin containers, and the integration tests use Testcontainers.
- (Optional) [Ollama](https://ollama.ai/) for the SemanticKernel AI opponent.
- (Optional) [K6](https://k6.io/) for load testing (or use the Docker wrapper in
  `tests/BattleshipGame.LoadTests`).

## Quick Start

```bash
dotnet run --project src/BattleshipGame.AppHost
```

This brings up the full local stack:

- **PostgreSQL** (with pgAdmin) provisioned by Aspire.
- **MigrationRunner** (`src/BattleshipGame.MigrationRunner`) applies EF Core migrations and exits.
- **Web API** starts only after migrations complete (`WaitForCompletion`).

The Aspire dashboard URL is printed in the console at startup. Use it to view resource health,
logs, and endpoints (including the Web API URL and Swagger UI).

The Web API, PostgreSQL connection, and migration ordering are wired in
[`src/BattleshipGame.AppHost/Program.cs`](../src/BattleshipGame.AppHost/Program.cs). The database
resource is named `battleship`; the same connection-string name is used by the Web API and the
MigrationRunner.

## AI Opponent (optional)

The SemanticKernel opponent talks to an OpenAI-compatible endpoint. For local development, run
Ollama and configure the endpoint via user secrets or environment variables:

```bash
export OPENAI_MODEL_ID=llama3.2
export OPENAI_ENDPOINT=http://localhost:11434/v1
export OPENAI_API_KEY=ollama
```

If unset, games using the `Random` opponent strategy still work without any AI backend.

## Database Migrations

Migrations live in the **Infrastructure** project under
`src/BattleshipGame.Infrastructure/Persistence/Migrations/` and are applied automatically by the
MigrationRunner when you start the AppHost — no manual step is required for normal development.

After changing the EF model, create a new migration:

```bash
dotnet tool restore   # ensures dotnet-ef is available

dotnet ef migrations add <MigrationName> \
  --project src/BattleshipGame.Infrastructure \
  --startup-project src/BattleshipGame.MigrationRunner
```

Never hand-edit the generated model snapshot — regenerate it through the EF tooling.

## Testing

```bash
# Unit tests (no container runtime needed)
dotnet test tests/BattleshipGame.UnitTests

# Integration tests — require a running container runtime.
# They spin up PostgreSQL via Testcontainers and run migrations against it.
dotnet test tests/BattleshipGame.IntegrationTests
```

## Load Testing

K6 load tests target `http://host.docker.internal:5000` by default. Point `API_BASE_URL` at the
Web API endpoint reported by the Aspire dashboard:

```bash
cd tests/BattleshipGame.LoadTests
npm run test:smoke
npm run test:load
```

## Authentication

JWT bearer authentication (Keycloak) is planned but not yet implemented — see
[`specs/delivery-plan.md`](../specs/delivery-plan.md) Phase 3. This guide will be updated with the
local auth flow once that work lands.
