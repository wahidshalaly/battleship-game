# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Purpose

This is a learning playground for DDD, AI-powered applications, and engineering acceleration with AI. The game itself is secondary — the primary goal is exploring patterns and architecture.

## Commands

```bash
# Build
dotnet build

# Run the Web API (default port 5000)
dotnet run --project src/BattleshipGame.WebAPI

# Run via .NET Aspire (orchestrates all services)
dotnet run --project src/BattleshipGame.AppHost

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/BattleshipGame.UnitTests
dotnet test tests/BattleshipGame.IntegrationTests

# Run a single test by name filter
dotnet test --filter "FullyQualifiedName~MethodName"

# Format code (CSharpier)
dotnet csharpier .

# Restore local tools (includes swagger CLI)
dotnet tool restore

# Generate OpenAPI docs
dotnet build ./src/BattleshipGame.WebAPI/BattleshipGame.WebAPI.csproj
dotnet swagger tofile --output ./docs/openapi.yaml --yaml ./src/BattleshipGame.WebAPI/bin/Debug/net10.0/BattleshipGame.WebAPI.dll v1
```

### Load Tests (K6 via Docker)

```bash
cd tests/BattleshipGame.LoadTests
npm run test:smoke          # 1 VU, 1 minute — run after deployments
npm run test:load           # 0-50 VUs, ~10 minutes
npm run test:full-game      # Complete game playthrough (Random strategy)
npm run test:full-game:sk   # Complete game playthrough (SemanticKernel strategy)
```

K6 tests require Docker and the API running at `http://host.docker.internal:5000`.

### Frontend (BattleshipGame.Web)

```bash
cd src/BattleshipGame.Web
npm install                 # first time only
npm run dev                 # Vite dev server on http://localhost:5173
npm run build               # type-check + production build
npm run test                # Vitest suite
npm run lint                # oxlint
npm run format              # Prettier
npm run generate:api-types  # regenerate src/api/schema.d.ts from docs/openapi.yaml
```

The frontend runs standalone (not orchestrated by Aspire). Start the backend with
`dotnet run --project src/BattleshipGame.AppHost`, then `npm run dev` in the Web project.
It calls the API at `VITE_API_BASE_URL` (default `http://localhost:5298`); the API allows
the `http://localhost:5173` origin via `Cors:AllowedOrigins`. Regenerate the API types
whenever `docs/openapi.yaml` changes.

## Architecture

### Layer Structure

```
BattleshipGame (src/)
├── Domain           — Entities, value objects, aggregates, domain events (no dependencies)
├── Application      — CQRS handlers, application services, repository contracts, AI opponent contracts
├── Infrastructure   — Repository implementations (in-memory), AI opponent strategies, Polly resilience
├── WebAPI           — ASP.NET Core controllers, middleware, DI wiring
├── Web              — React + Vite SPA (TypeScript, Tailwind, TanStack Query)
├── AppHost          — .NET Aspire orchestration entry point
└── ServiceDefaults  — Shared Aspire service defaults

tests/
├── UnitTests        — Domain and application layer tests (Arrange-Act-Assert, FakeItEasy)
├── IntegrationTests — Full API simulation tests
└── LoadTests        — K6 load/stress/spike/soak scenarios
```

### Key Flows

**Attack flow**: `POST /api/games/{id}/attacks` → `GameplayService.PlayerAttackThenCounterAttackAsync` → `PlayerAttackCommand` → `Game.Attack(BoardSide.Opponent)` → if game not over → `OpponentAttackCommand` → `Game.Attack(BoardSide.Player)` → returns `LastRoundResult` containing both attack outcomes.

**AI opponent selection**: `OpponentStrategy` enum is stored on `Game` at creation time. `OpponentStrategyFactory` uses .NET keyed DI to resolve either `RandomAttackOpponent` or `SemanticKernelOpponent`. The `ResilientComputerOpponentDecorator` wraps the SK strategy with Polly retry + circuit breaker for HTTP 429 and general `AiOpponentException`.

**Domain events**: Aggregates call `AddDomainEvent(...)`, then the application layer calls `IDomainEventDispatcher.DispatchEventsAsync(aggregate)`, which publishes each event through MediatR to registered `INotificationHandler<T>` handlers.

### CQRS Pattern

- Commands in `Application/Features/Games/Commands/` and `Application/Features/Players/Commands/`
- Queries in `Application/Features/Games/Queries/` and `Application/Features/Players/Queries/`
- Commands return rich result objects (`AttackResult`, `LastRoundResult`) — no follow-up query needed
- `GamesController` mixes direct `IMediator.Send()` for simple reads with `IGameplayService` for orchestrated multi-step flows

### AI Opponent Configuration

The `OpenAi` section in `appsettings.json` (or user secrets) configures the LLM endpoint. For local development, Ollama is expected at the configured URL. The default dev model is `llama-3.3-70b-versatile`. The SK strategy builds prompts via `BattleshipPromptBuilder` using `GameStateContext` (a read-only game state projection).

## Conventions

### C# Style
- Primary constructor syntax for DI: `public class Foo(IBar bar) { ... }`
- `record` types for immutable data (DTOs, value objects, strongly-typed IDs)
- `var` when type is obvious; explicit type otherwise
- `Async` suffix on all async methods
- `nameof()` in exceptions and logging
- `switch` expressions for concise conditionals; pattern matching for type checks
- Expression-bodied members for simple getters/methods
- String interpolation over `String.Format`

### Architecture Rules
- No logic in controllers — delegate to `IGameplayService` or MediatR commands
- No static state or service locators
- Config via `appsettings.json` and `IOptions<T>`, never hardcoded
- Never expose domain entities directly in API responses — use DTOs
- Domain layer has zero dependencies on other layers or NuGet packages beyond the BCL

### Testing
- xUnit + FluentAssertions + FakeItEasy
- Arrange-Act-Assert structure
- Test edge cases and exceptions explicitly

### Frontend (Web)
- Typed API access via `openapi-typescript` (generated `src/api/schema.d.ts`) + `openapi-fetch`; feature code calls the thin wrappers in `src/api/endpoints/*`, never the raw client
- Server state via TanStack Query; local component state with `useState` — no Redux/Zustand
- Styling with Tailwind CSS v4 utility classes; oxlint + Prettier (scoped `.prettierrc`, not CSharpier)
- Tests: Vitest + React Testing Library; favor pure-logic tests (placement, round history)
- Auth tokens live in `localStorage` + an in-memory mirror (pragmatic for a learning project; an httpOnly-cookie BFF would be the production-grade alternative). The `AuthController` is a full façade over Keycloak — the SPA never talks to Keycloak directly
- The API requires the client to place **both** boards before a game is `Ready`; the frontend auto-generates random opponent placements the player never sees. Ship lengths are mirrored from `BattleshipGame.Domain` (no API exposes them)

## Documentation

Design docs live in `docs/` and should be kept in sync with significant code changes:
- `docs/architecture.md` — layer responsibilities, patterns, data flow diagrams
- `docs/design.md` — class diagrams and component relationships
- `docs/analysis.md` — domain model and business rules
- `docs/bounded-context-analysis.md` — bounded context alignment assessment

For SDK documentation, use Context7 (as configured for Copilot).

## Formatting

CSharpier is the formatter with `printWidth: 100` and no Prettier-style trailing commas. Run `dotnet csharpier .` before committing.
