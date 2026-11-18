# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Purpose

This is a learning playground for DDD, AI-powered applications, and engineering acceleration with AI. The game itself is secondary — the primary goal is exploring patterns and architecture.

## Commands

```bash
# Build
dotnet build

# Run everything via .NET Aspire: PostgreSQL, Keycloak, migrations, Web API, frontend
dotnet run --project src/BattleshipGame.AppHost

# Run the Web API alone (http://localhost:5298); needs PostgreSQL, Keycloak and the OpenAi settings
dotnet run --project src/BattleshipGame.WebAPI

# Run all tests (integration tests need Docker for Testcontainers; no LLM needed)
dotnet test

# Run a single test project
dotnet test tests/BattleshipGame.UnitTests
dotnet test tests/BattleshipGame.IntegrationTests

# Run a single test by name filter
dotnet test --filter "FullyQualifiedName~MethodName"

# Restore local tools (CSharpier, Swagger CLI, dotnet-ef)
dotnet tool restore

# Format code (CSharpier)
dotnet csharpier format .
dotnet csharpier check .
```

### Database Migrations (EF Core)

```bash
dotnet ef migrations add <Name> \
  --project src/BattleshipGame.Infrastructure \
  --startup-project src/BattleshipGame.MigrationRunner \
  --output-dir Persistence/Migrations
```

`BattleshipGame.MigrationRunner` applies migrations (Aspire runs it before the API starts). `dotnet ef migrations remove` needs a database connection; to undo an unapplied migration, delete its two files and restore `BattleshipGameDbContextModelSnapshot.cs` from git.

### OpenAPI Document

```bash
dotnet build ./src/BattleshipGame.WebAPI/BattleshipGame.WebAPI.csproj
cd src/BattleshipGame.WebAPI
dotnet swagger tofile --output ../../docs/openapi.yaml --yaml bin/Debug/net11.0/BattleshipGame.WebAPI.dll v1
```

The API validates its options at startup, so `swagger tofile` needs placeholder values for `Authentication__Authority`, `Authentication__Audience`, `Keycloak__*` and `OpenAi__*` (nothing connects). Then regenerate the frontend types (`npm run generate:api-types`). A document generated on Windows differs only in `\r\n` line endings — don't commit that noise.

### Load Tests (K6 via Docker)

```bash
cd tests/BattleshipGame.LoadTests
npm run test:smoke          # 1 VU, 1 minute — run after deployments
npm run test:load           # 0-50 VUs, ~10 minutes
npm run test:full-game      # Complete game playthrough (Random strategy)
npm run test:full-game:sk   # Complete game playthrough (SemanticKernel strategy)
```

K6 tests require Docker and the API reachable from the container (set `API_BASE_URL`, e.g. `http://host.docker.internal:5298`).

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

Aspire starts the frontend on `http://localhost:5173`; `npm run dev` runs it on its own. It calls the API at `VITE_API_BASE_URL` (default `http://localhost:5298`); the API allows the `http://localhost:5173` origin via `Cors:AllowedOrigins`. Regenerate the API types whenever `docs/openapi.yaml` changes.

## Architecture

### Layer Structure

```
BattleshipGame (src/)
├── Domain           — Aggregates, entities, value objects, domain events (no dependencies)
├── Application      — CQRS handlers, application services, repository contracts, AI opponent contracts
├── Infrastructure   — EF Core + PostgreSQL persistence, Keycloak identity, AI opponent strategies, Polly resilience
├── WebAPI           — ASP.NET Core controllers, middleware, validation, DI wiring
├── Web              — React + Vite SPA (TypeScript, Tailwind, TanStack Query)
├── MigrationRunner  — Applies EF Core migrations
├── AppHost          — .NET Aspire orchestration entry point
└── ServiceDefaults  — Shared Aspire service defaults

tests/
├── UnitTests        — Domain and application layer tests (Arrange-Act-Assert, FakeItEasy)
├── IntegrationTests — Full API tests against PostgreSQL (Testcontainers), scripted LLM fake
└── LoadTests        — K6 load/stress/spike/soak scenarios
```

### Domain Model

- **`Game` aggregate** — boards, ships, cells, attacks and state transitions (`New → Ready → Started → GameOver`). References its owner by `PlayerId`.
- **`Player` aggregate** — identity only (`Username`, `IdentitySubject`, the Keycloak `sub`). A player's active game (the one not `GameOver`) and history are queries over games, never stored on the player.
- **One active game per player** — checked by `StartNewGame` (`PlayerHasActiveGameException` → 409 with `activeGameId`) and enforced by a filtered unique index on `games.player_id`.

### Key Flows

**Attack flow**: `POST /api/games/{id}/attacks` → `GameplayService.PlayerAttackThenCounterAttackAsync` → `PlayerAttackCommand` → `Game.Attack(BoardSide.Opponent)` → if game not over → `OpponentAttackCommand` → `Game.Attack(BoardSide.Player)` → returns `LastRoundResult` containing both attack outcomes.

**AI opponent selection**: `OpponentStrategy` enum is stored on `Game` at creation time. `ComputerOpponentFactory` uses .NET keyed DI to resolve either `RandomAttackOpponent` or `LlmOpponent`. `LlmOpponent` builds the prompts and parses the reply; it calls the model through `ILlmChat`, implemented by `SemanticKernelChat` (the only class that uses Semantic Kernel), which integration tests replace with a scripted fake. The `ResilientComputerOpponentDecorator` wraps the LLM strategy with Polly retry + circuit breaker for HTTP 429 and general `AiOpponentException`, falling back to `RandomAttackOpponent`.

**Domain events**: Aggregates call `AddDomainEvent(...)`, then the application layer calls `IDomainEventDispatcher.DispatchEventsAsync(aggregate)`, which publishes each event through MediatR to registered `INotificationHandler<T>` handlers. The dispatcher is scoped, so handlers share the request's `DbContext` and their changes commit with the command (`UnitOfWorkBehavior`).

### CQRS Pattern

- Commands in `Application/Features/Games/Commands/` and `Application/Features/Players/Commands/`
- Queries in `Application/Features/Games/Queries/` and `Application/Features/Players/Queries/`
- Commands return rich result objects (`AttackResult`, `LastRoundResult`) — no follow-up query needed
- `UnitOfWorkBehavior` commits after every request, so a command that changes several aggregates commits atomically
- `GamesController` mixes direct `IMediator.Send()` for simple reads with `IGameplayService` for orchestrated multi-step flows

### AI Opponent Configuration

The API binds the LLM settings to `OpenAiOptions` (section `OpenAi`: `ModelId`, `Endpoint`, `ApiKey`) and refuses to start without them. For local development, `appsettings.Development.json` points at Ollama (endpoint `http://localhost:11434/v1`, model `gemma4`, any API key) and integration tests use placeholders from `appsettings.Test.json`; see `docs/local-dev.md`. Hosted environments can use `OpenAi__*` environment variables. `BattleshipPromptBuilder` builds the prompts from `GameSnapshot` (a read-only game state projection).

## Conventions

### C# Style
- Idiomatic C# following the Microsoft .NET coding conventions; prioritise readability, testability and SOLID
- Naming: PascalCase for types, methods and properties; camelCase for locals and parameters; `_camelCase` for private fields; `I` prefix for interfaces
- Nullable reference types are enabled in every project; async/await end to end
- Named methods instead of anonymous lambdas in business logic (short LINQ and EF predicates are fine)
- `using` declarations for `IDisposable` objects
- Primary constructor syntax for DI: `public class Foo(IBar bar) { ... }`
- `record` types for immutable data (DTOs, query results, strongly-typed IDs); domain value objects derive from `ValueObject`
- `var` when type is obvious; explicit type otherwise
- `Async` suffix on async methods, except where the framework fixes the name (MediatR `Handle`, controller actions)
- `nameof()` in exceptions and logging
- `switch` expressions for concise conditionals; pattern matching for type checks
- `??`, `?.` and throw expressions for null handling
- `using static` for constants (e.g. `using static BattleshipGame.Domain.Common.Constants`)
- Expression-bodied members for simple getters/methods
- String interpolation over `String.Format`; message templates (not interpolation) in log calls

### Comments and Documentation
- XML `///` doc comments document types and members; keep them complete and consistent with neighbouring code
- Inline `//` comments only for a non-obvious *why* — never to narrate what a line does or restate a name
- Tests keep the `// Arrange` / `// Act` / `// Assert` markers

### Architecture Rules
- Clean Architecture: dependencies point inward (WebAPI, Infrastructure → Application → Domain)
- Dependency injection for services and repositories, registered per layer in `ServiceCollectionExtensions`
- No logic in controllers — delegate to `IGameplayService` or MediatR commands
- No static state or service locators
- Config via `appsettings.json` and `IOptions<T>`, never hardcoded
- Never expose domain entities directly in API responses — use DTOs, mapped by hand (static `From` factories, `ToResponse`); no AutoMapper
- Avoid fat controllers and God classes
- Structured logging with `ILogger<T>`
- Domain layer has zero dependencies on other layers or NuGet packages beyond the BCL
- Aggregates reference each other by ID, in one direction only (`Game → Player`)
- Derive state from its owning aggregate instead of copying it into another; a copy needs syncing and drifts

### API Conventions
- `[ApiController]`, `ActionResult<T>` and `[ProducesResponseType]` for every response, including errors
- Errors are ProblemDetails produced by `ExceptionHandlingMiddleware`, which maps domain exceptions to status codes (404 not found, 409 conflicts, 400 invalid requests)
- Input validation with FluentValidation validators in `WebAPI/Validation`, applied by `FluentValidationActionFilter`
- After changing endpoints or response types, regenerate `docs/openapi.yaml` and the frontend types

### Testing
- xUnit + FluentAssertions + FakeItEasy
- Arrange-Act-Assert structure
- Prefer TDD for business logic: write the failing test first, especially when fixing a bug
- Test edge cases and exceptions explicitly
- Integration tests never call a real LLM: they script `FakeLlmChat`, and `VerifyAllCallsScripted` fails a test that hit the Random fallback by accident

### Frontend (Web)
- Typed API access via `openapi-typescript` (generated `src/api/schema.d.ts`) + `openapi-fetch`; feature code calls the thin wrappers in `src/api/endpoints/*`, never the raw client
- Server state via TanStack Query; local component state with `useState` — no Redux/Zustand
- Styling with Tailwind CSS v4 utility classes; oxlint + Prettier (scoped `.prettierrc`, not CSharpier)
- Tests: Vitest + React Testing Library; favor pure-logic tests (placement, round history)
- Auth tokens live in `localStorage` + an in-memory mirror (pragmatic for a learning project; an httpOnly-cookie BFF would be the production-grade alternative). The `AuthController` is a full façade over Keycloak — the SPA never talks to Keycloak directly
- The API requires the client to place **both** boards before a game is `Ready`; the frontend auto-generates random opponent placements the player never sees. Ship lengths are mirrored from `BattleshipGame.Domain` (no API exposes them)

## Working Agreements

- **PR scope:** if a change creates the need for a follow-up (it leaves the model inconsistent otherwise), do it in the same PR as separate commits. Open a separate issue/PR only for work with independent value, risky changes, or when the PR gets too big to review.
- **Commits:** one concern per commit, with a message that explains why.
- **Issues:** if the approach changes during implementation, update the issue (resolution note, superseded plan, accurate acceptance criteria) so it doesn't mislead later readers.

## Documentation

Design docs live in `docs/` and must be kept in sync with significant code changes:
- `docs/architecture.md` — layer responsibilities, patterns, data flow diagrams
- `docs/design.md` — class diagrams and component relationships
- `docs/analysis.md` — domain model and business rules
- `docs/bounded-context-analysis.md` — bounded context alignment assessment
- `docs/local-dev.md` — local setup (Aspire, Keycloak, Ollama)
- `docs/gameplay.md` — how to play
- `docs/openapi.yaml` — generated API description (see above)

For SDK and library documentation, use Context7.

## Formatting

CSharpier is the formatter with `printWidth: 100` and no Prettier-style trailing commas. Use CSharpier, not `dotnet format`. Run `dotnet csharpier format .` before committing (a Claude Code hook in `.claude/settings.json` also runs it after every edit).
