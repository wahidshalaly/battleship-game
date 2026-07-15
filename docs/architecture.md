# Battleship Game - Architecture Overview

## Executive Summary

The Battleship Game is a web-based implementation of the classic naval strategy game, built using **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns. The system is designed to be maintainable, testable, and extensible.

## Technology Stack

### Backend
- **.NET 10**: Latest version of .NET
- **ASP.NET Core**: Web API framework
- **C# 13**: Modern C# features with nullable reference types
- **Keycloak 26**: OIDC identity provider (JWT issuance, token validation)
- **Swagger/OpenAPI**: API documentation and testing

### Frontend (BattleshipGame.Web)
- **React 19 + TypeScript**: SPA framework
- **Vite**: Dev server and build tool
- **Tailwind CSS v4**: Styling
- **TanStack Query**: Server-state caching
- **openapi-typescript + openapi-fetch**: Typed API client generated from `docs/openapi.yaml`
- **Vitest + React Testing Library**: Frontend tests

The SPA runs standalone (`npm run dev` on port 5173) against the Web API, which allows its
origin via CORS. It authenticates through the API's `AuthController` façade — the browser
never contacts Keycloak directly (client → API `/api/auth/*` → Keycloak). Access/refresh
tokens are held in `localStorage`, sent as `Authorization: Bearer` on each request, with a
single automatic refresh-and-retry on a 401.

### Testing
- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable test assertions
- **FakeItEasy**: Mocking framework
- **Testcontainers**: Real Postgres and Keycloak containers in integration tests

### Development Tools
- **CSharpier**: Code formatting
- **Docker**: Containerization support
- **Visual Studio/Rider**: IDE support

## Architectural Principles

### Clean Architecture
The system follows Uncle Bob's Clean Architecture pattern with clear separation of concerns:

```mermaid
graph TB
    Web[React SPA] --> UI[Web API Layer]
    UI[Web API Layer] --> App[Application Layer]
    App --> Domain[Domain Layer]
    App --> Infra[Infrastructure Layer]
    Infra --> Domain

    Web -.-> |"HTTP + JWT bearer"| Web
    UI -.-> |"DTOs, Controllers"| UI
    App -.-> |"Services, Use Cases"| App
    Domain -.-> |"Entities, Value Objects, Business Rules"| Domain
    Infra -.-> |"Repositories, External Services"| Infra
```

### Domain-Driven Design (DDD)
- **Ubiquitous Language**: Consistent terminology across code and business
- **Bounded Contexts**: Clear boundaries around related concepts
- **Aggregates**: Consistency boundaries with aggregate roots
- **Value Objects**: Immutable objects without identity
- **Domain Events**: Decoupled communication pattern

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes must be substitutable
- **Interface Segregation**: Many specific interfaces vs. one general
- **Dependency Inversion**: Depend on abstractions, not concretions

## Layer Responsibilities

### 1. Domain Layer (`BattleshipGame/Domain`)
**Purpose**: Contains the core business logic and domain model.

**Components**:
- **Entities**: `Game`, `Board`, `Ship` (with identity and lifecycle)
- **Value Objects**: `Cell`, strongly-typed IDs (immutable objects)
- **Aggregates**: `GameAggregate`, `PlayerAggregate` (consistency boundaries)
- **Domain Events**: Event-driven communication via `IDomainEvent`, `DomainEvent<T>` base classes
- **Business Rules**: Game logic, validation, and constraints

**Domain Events Implemented**:
- `CellAttackedEvent`: Raised when a cell is attacked
- `GameOverEvent`: Raised when game concludes
- `ShipSunkEvent`: Raised when a ship is destroyed
- `BoardsReadyEvent`: Raised when both boards are ready for gameplay
- `PlayerJoinedGameEvent`: Raised when player joins game
- `PlayerLeftGameEvent`: Raised when player leaves game

**Dependencies**: None (pure domain logic)

### 2. Application Layer (`BattleshipGame/Application`)
**Purpose**: Orchestrates domain objects to fulfill use cases using CQRS pattern.

**Components**:
- **Application Services**:
  - `IGameplayService`, `GameplayService` — Orchestrates game lifecycle
  - `IPlayerService`, `PlayerService` — Manages player operations and current-caller lookup (`GetCurrentAsync`, `GetCurrentRequiredAsync`, `CreateAsync`)
- **Commands**: `CreateGameCommand`, `PlaceShipCommand`, `PlayerAttackCommand`, `OpponentAttackCommand`, `CreatePlayerCommand` (handlers via MediatR)
- **Queries**: `GetGameQuery`, `GetPlayerQuery`, `GetPlayerByUsernameQuery`, `GetPlayerByIdentitySubjectQuery` (handlers via MediatR)
- **DTOs**: Data transfer objects for inter-layer communication (`GetGameQueryResult`, `GetPlayerQueryResult`)
- **Result Types**: Rich result objects from commands (`AttackResult`, `LastRoundResult`)
- **Repository Contracts**: `IGameRepository`, `IPlayerRepository` (abstraction for persistence)
- **Identity Contracts**: `IIdentityProvider` — abstraction over the external IdP (Keycloak in production/tests)
- **Domain Event Dispatcher**: `IDomainEventDispatcher` — Publishes domain events through MediatR
- **Security Contracts**: `ICurrentUser` — resolves the authenticated caller's identity subject from the HTTP context

**CQRS Command Pattern**:
- Commands return rich result objects containing mutation outcomes (CQRS-compliant)
- `AttackResult` contains attack details: `TargetCell`, `CellState`, `GameState`, `WinnerSide`, `SunkShip`, `ShipSize`
- `LastRoundResult` contains complete round outcome: both player and opponent attack details
- Commands eliminate need for follow-up queries (e.g., removed `CheckGameStatusCommand`)
- Results are constructed from domain events and aggregate state in command handlers

**Event Dispatch Pattern**:
- Domain events are raised within aggregate roots using `AddDomainEvent()`
- `DomainEventDispatcher` publishes events to MediatR for decoupled handling
- Event handlers can implement cross-cutting concerns (notifications, logging, etc.)
- Aggregates maintain `IReadOnlyList<IDomainEvent>` of pending events

**Opponent Contracts**:
- `IComputerOpponent`: Strategy interface for AI attack selection (always targets Player's board)
- `IComputerOpponentFactory`: Factory for per-game strategy resolution
- `IPromptBuilder`: LLM prompt construction for SemanticKernel strategy
- `GameStateContext`: Read-only projection for AI decision-making (built by strategy internally)

**Dependencies**: Domain Layer only

### 3. Infrastructure Layer (`BattleshipGame/Infrastructure`)
**Purpose**: Implements external concerns and data persistence.

**Components**:
- **Repositories** (EF Core + PostgreSQL):
  - `GameRepository`: Implements `IGameRepository` over `BattleshipGameDbContext`
  - `PlayerRepository`: Implements `IPlayerRepository` over `BattleshipGameDbContext`
  - See the **Persistence (EF Core + PostgreSQL)** section below for the data model
- **Identity**:
  - `KeycloakIdentityProvider`: Implements `IIdentityProvider` via Keycloak Admin REST API + ROPC token endpoint
  - Handles `RegisterAsync` (create user without inline credentials → `/users/{id}/reset-password`), `SignInAsync` (ROPC grant), `RefreshAsync`, `SignOutAsync`
  - `KeycloakOptions`: config binding for `Keycloak` section (`BaseUrl`, `Realm`, `ClientId`, `ClientSecret`, `AdminUsername`, `AdminPassword`)
- **Opponent Strategies**:
  - `RandomAttackStrategy`: Random cell selection from available targets on Player's board
  - `SemanticKernelStrategy`: LLM-based strategic attack selection using Semantic Kernel
  - `OpponentStrategyFactory`: Factory resolving strategies via keyed DI services
  - `BattleshipPromptBuilder`: Constructs prompts for LLM-based strategies
- **Data Access**: `BattleshipGameDbContext` (EF Core 10 + Npgsql) with entity configurations and migrations
- **External Services**: Semantic Kernel integration for AI/LLM features; Keycloak Admin REST API for identity management

**Repository Pattern Benefits**:
- Abstracts data access details from application layer
- Enables easy switching between in-memory and persistent storage
- Facilitates testing through mock implementations
- Provides consistent data access interface

**Dependencies**: Domain Layer, Application Layer

### 4. Presentation Layer (`BattleshipGame.WebAPI`)
**Purpose**: Handles HTTP requests and responses.

**Components**:
- **Controllers**:
  - `AuthController` — identity façade: `POST /api/auth/register`, `POST /api/auth/signin`, `POST /api/auth/refresh`, `POST /api/auth/logout` (all `[AllowAnonymous]`)
  - `PlayersController` — `GET /api/players/me` (caller's own profile)
  - `GamesController` — game lifecycle endpoints + `GET /api/games/active`
- **DTOs**: Request/response models (e.g., `RegisterRequest`, `SignInRequest`, `AuthTokenResponse`, `CreateGameRequest`)
- **Middleware**: `ExceptionHandlingMiddleware` — maps domain/application exceptions to HTTP status codes (401 `InvalidCredentialsException`, 403 `ForbiddenAccessException`, 404 `NotFoundException`, 409 `IdentityConflictException`)
- **Validation**: `FluentValidation` validators per request type
- **Configuration**: DI wiring, Swagger, JWT bearer (`AddJwtBearer` pointed at `Authentication:Authority`), global `RequireAuthorization` fallback policy

**Dependencies**: Application Layer, Infrastructure Layer

## Key Design Patterns

### Repository Pattern
Abstracts data access to enable testability and flexibility:
```csharp
public interface IGameRepository
{
    Task<Game?> GetByIdAsync(GameId gameId, CancellationToken ct);
    Task<Game> GetByIdOrThrowAsync(GameId gameId, CancellationToken ct);
    Task SaveAsync(Game game, CancellationToken ct);
    Task<IReadOnlyCollection<Game>> GetByPlayerIdAsync(PlayerId playerId, CancellationToken ct);
    Task<Game?> GetActiveGameByPlayerIdAsync(PlayerId playerId, CancellationToken ct);
}

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken ct);
    Task<PlayerId> SaveAsync(Player player, CancellationToken ct);
    Task<Player?> GetByUsernameAsync(string username, CancellationToken ct);
    Task<Player?> GetByIdentitySubjectAsync(string subject, CancellationToken ct);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
}
```

### Aggregate Pattern
Maintains consistency boundaries and encapsulates business rules:
```csharp
public sealed class Game : AggregateRoot<GameId>
{
    // Encapsulates game rules and state
    // Controls access to internal entities (Board, Ship)
    // Ensures invariants are maintained
}
```

### Strongly-Typed IDs
Prevents primitive obsession and improves type safety:
```csharp
public record GameId(Guid Value) : EntityId(Value);
public record ShipId(Guid Value) : EntityId(Value);
```

### Domain Events Pattern
Enables loose coupling and cross-cutting concerns:
```csharp
public abstract class DomainEvent<T> : IDomainEvent
    where T : class
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Type EventType { get; init; } = typeof(T);
}
```

**Event Flow**:
1. Aggregate raises domain event: `AddDomainEvent(new ShipSunkEvent(...))`
2. Event is stored in aggregate's `DomainEvents` collection
3. Application layer dispatches events: `await eventDispatcher.DispatchEventsAsync(aggregate)`
4. `DomainEventDispatcher` publishes each event through MediatR
5. MediatR routes events to corresponding event handlers
6. Handlers implement side effects (logging, notifications, state updates)
7. Aggregate clears events: `aggregate.ClearDomainEvents()`

**Benefits**:
- Decouples domain from application concerns
- Enables audit trail and event replay capabilities
- Supports event sourcing (future enhancement)
- Clean separation of core logic from side effects

**Command Return Values**:
- Commands can return mutation outcomes without violating CQRS principles
- Return values represent **what changed**, not queries of current state
- Example: `AttackResult` returns attack outcome (hit/miss), sunk ship info, and game state changes
- Eliminates need for follow-up queries (better performance, simpler code)
- Pattern: Command executes → Domain events raised → Result built from events and aggregate state

## Data Flow

### Game Attack Flow (Player Attack + Counter-Attack)
```mermaid
sequenceDiagram
    participant Client
    participant API as GamesController
    participant Service as GameplayService
    participant PlayerCmd as PlayerAttack
    participant OpponentCmd as OpponentAttack
    participant Domain as Game

    Client->>API: POST /api/games/{id}/attacks
    API->>Service: PlayerAttackThenCounterAttackAsync(gameId, cell)
    Service->>PlayerCmd: Handle command
    PlayerCmd->>Domain: Attack(BoardSide.Opponent, cell)
    Domain->>Domain: Validate & attack
    Domain-->>PlayerCmd: Result
    PlayerCmd-->>Service: AttackResult
    
    alt Game Not Over
        Service->>OpponentCmd: Handle command
        OpponentCmd->>Domain: Attack(BoardSide.Player, cell)
        Domain->>Domain: Validate & attack
        Domain-->>OpponentCmd: Result
        OpponentCmd-->>Service: AttackResult
    end
    
    Service-->>API: LastRoundResult
    API-->>Client: 200 OK
```

### Game Creation Flow
```mermaid
sequenceDiagram
    participant Client
    participant API as GamesController
    participant Service as GameService
    participant Domain as Game
    participant Repo as IGameRepository

    Client->>API: POST /api/games
    API->>Service: StartGame(playerId, boardSize)
    Service->>Domain: new Game(playerId, boardSize)
    Domain-->>Service: Game instance
    Service->>Repo: SaveAsync(game)
    Repo-->>Service: Success
    Service-->>API: GameId
    API-->>Client: 201 Created
```

### Ship Placement Flow
```mermaid
sequenceDiagram
    participant Client
    participant API as GamesController
    participant Domain as Game
    participant Board
    participant Ship

    Client->>API: POST /api/games/{id}/ships
    API->>Domain: PlaceShip(side, kind, orientation, bow)
    Domain->>Board: PlaceShip(kind, orientation, bow)
    Board->>Board: ValidateBeforePlaceShip(...)
    Board->>Ship: new Ship(kind, position)
    Ship-->>Board: Ship instance
    Board-->>Domain: ShipId
    Domain-->>API: ShipId
    API-->>Client: 200 OK
```

## Persistence (EF Core + PostgreSQL)

Persistence is implemented with **Entity Framework Core 10** on **PostgreSQL** (Npgsql), living entirely in the Infrastructure layer. The Domain and Application layers depend only on the `IGameRepository` / `IPlayerRepository` contracts and stay persistence-agnostic.

### DbContext and tables

`BattleshipGameDbContext` exposes **internal** `DbSet`s (only the repositories touch them) and applies all `IEntityTypeConfiguration` types from the assembly. There are three tables:

| Aggregate / entity | Table | Notes |
|---|---|---|
| `PlayerEntity` | `players` | Unique index on `username`; filtered unique index on `active_game_id` |
| `PlayerGameHistoryEntry` | `player_game_history` | Child of player (FK `player_id`, cascade delete) |
| `GameEntity` | `games` | Owns both boards as JSON (see below) |

There are **no separate Board / Cell / Ship tables**. Each `GameEntity` owns two boards (`OwnBoard`, `OppBoard`) mapped with `.ToJson()` into the `own_board` / `opp_board` `jsonb` columns; each board in turn owns its `Cells` and `Ships` collections, serialized inside that same JSON document. EF assigns a synthetic ordinal (`__synthesizedOrdinal`) as the key of these JSON-owned collection elements.

### Mapping choices

- **Strongly-typed IDs** (`GameId`, `PlayerId`) are stored as raw `Guid` columns; conversion to/from the domain types happens in the repositories' mapping methods (`MapToDomain` / `MapToEntity`), so no EF concerns leak into the Domain.
- **Enums** (`State`, `OpponentStrategy`, board sides) are stored as `int`.
- **Optimistic concurrency**: both `players` and `games` use PostgreSQL's system `xmin` column as a row-version concurrency token (`IsRowVersion()`). A writer that loses a concurrent-update race gets a `DbUpdateConcurrencyException`.
- Columns use `snake_case` names.

### Unit of work

Repositories **stage** changes (add or mutate tracked entities) but do not call `SaveChanges` themselves. The MediatR `UnitOfWorkBehavior` commits the `DbContext` once per request, so a command that touches multiple aggregates (e.g. `StartNewGame`, which saves both a `Game` and a `Player`) is persisted atomically.

### Migrations and local run

- Migrations live in `src/BattleshipGame.Infrastructure/Persistence/Migrations`.
- A dedicated `BattleshipGame.MigrationRunner` console project applies migrations.
- The Aspire AppHost provisions PostgreSQL, runs the MigrationRunner, and starts the Web API only after migrations complete (`WaitForCompletion`). See [local-dev.md](local-dev.md).

DI wiring (`AddInfrastructureServices`) registers `AddDbContext<BattleshipGameDbContext>` with `UseNpgsql(...)` against the `battleship` connection string, plus scoped `IGameRepository`, `IPlayerRepository`, and `IUnitOfWork`.

## Error Handling Strategy

### Domain Layer
- Throws `ArgumentException` for invalid inputs
- Throws `ApplicationException` for business rule violations
- Uses centralized `ErrorMessages` class for consistency

### Application Layer
- Catches domain exceptions and translates to appropriate responses
- Validates inputs before calling domain methods
- Logs errors for debugging and monitoring

### API Layer
- Returns appropriate HTTP status codes (200, 201, 400, 404, 500)
- Uses Problem Details format for error responses
- Implements global exception handling middleware

## Testing Strategy

### Unit Testing (`BattleshipGame.UnitTests`)
- **Domain Layer**: Comprehensive coverage of business rules, value objects, domain events
- **Application Layer**: Command/query handler behavior with FakeItEasy mocks

### Integration Testing (`BattleshipGame.IntegrationTests`)
Two test collections run against real containers (Testcontainers):

| Collection | Fixtures | Tests |
|---|---|---|
| `GameIntegration` | `PostgresFixture` (shared Postgres container) | `GameApiSimulationTests` — 71 game-mechanics tests using `TestAuthHandler` (fast; no Keycloak) |
| `AuthIntegration` | `PostgresFixture` + `KeycloakFixture` | `AuthApiTests` — 4 e2e tests against real Keycloak 26.1 JWT |

**`KeycloakFixture`** starts a Keycloak 26.1 container and configures the `battleship` realm programmatically via the Admin REST API (no file mounting):
1. Obtain admin token (master realm).
2. Create realm.
3. Disable default required actions (so fresh users can sign in immediately).
4. Create `battleship-api` confidential client with Direct Access Grants.
5. Read back the auto-generated client secret.

**`TestAuthHandler`** is a no-op bearer scheme for game-mechanics tests: any `X-Test-Sub` header value is accepted as the authenticated subject, avoiding the Keycloak round-trip cost.

**DB-seed helper** in `GameApiSimulationTests` inserts `Player` rows directly via `DbContext` so the removed `POST /api/players` endpoint is not needed.

### Test Patterns
- **Arrange-Act-Assert**: Clear test structure
- **FakeItEasy**: Mocking framework for unit tests
- **FluentAssertions**: Readable, precise assertions
- **Shared containers**: one Postgres / Keycloak container per test collection (not per test class)

## Performance Considerations

### Memory Management
- Use of value objects for immutable data
- Efficient collections (HashSet, Dictionary)
- Proper disposal of resources

### Scalability
- Stateless API design
- Repository pattern for data access optimization
- Caching strategies (future enhancement)

## Authentication & Identity

### Overview

Authentication is provided by **Keycloak 26** (OIDC). The WebAPI acts as an **identity façade** — callers interact only with our own endpoints; Keycloak is an implementation detail.

```
Client ──► POST /api/auth/register ──► KeycloakIdentityProvider ──► Keycloak Admin API
                                    └──► CreatePlayerCommand ──────► PlayerRepository
        ◄── 201 { accessToken, refreshToken }

Client ──► POST /api/auth/signin ──► ROPC token endpoint ──► { accessToken, refreshToken }
Client ──► POST /api/auth/refresh ──► refresh_token grant ──► new token pair
Client ──► POST /api/auth/logout ──► revoke endpoint ──► 204
```

### Token validation

Every non-auth endpoint requires a valid Keycloak-issued JWT Bearer token. `AddJwtBearer` is configured with:
- `Authority` = `{Keycloak base URL}/realms/{realm}` — used for OIDC discovery and public-key fetch
- `Audience` validation against the configured client ID
- A global `RequireAuthorization` fallback policy — no endpoint is accidentally public

The `sub` claim in the access token is the Keycloak user ID, stored as `Player.IdentitySubject`.

### Registration flow

1. `POST /api/auth/register` calls `KeycloakIdentityProvider.RegisterAsync`:
   a. Obtain admin token (master realm, `admin-cli`).
   b. Create user **without** inline credentials; supply `firstName`/`lastName` = username to satisfy Keycloak 26's Declarative User Profile (omitting them auto-applies `UPDATE_PROFILE` and blocks ROPC).
   c. Set password via `PUT /users/{id}/reset-password` with `temporary: false`.
   d. Sign in immediately via ROPC to return tokens to the caller.
2. `AuthController` then dispatches `CreatePlayerCommand(username, subject)` to create the game `Player`.

> **Known limitation — registration is not atomic.** The Keycloak identity is created before the
> `Player`. If the `Player` insert fails (e.g. duplicate username in the game DB, transient DB
> error) the Keycloak user is left orphaned: the caller can authenticate but gets 403 on game
> endpoints, and a retry of `register` returns 409 from Keycloak. There is no compensating
> rollback today. This is accepted for the learning-playground scope; a production build would add
> a compensating delete, an outbox/saga, or a "complete registration" reconciliation step.

### Game ownership

`GameAccessGuard` resolves the caller's `Player` via `IPlayerService.GetCurrentRequiredAsync`, then enforces:
- 403 if the caller has no `Player` profile.
- 403 if the caller is not the game's owner.
- 401 (JWT middleware) if no valid token is present.

## Security Considerations

### API Security
- All endpoints require JWT bearer authentication (global `RequireAuthorization` fallback policy)
- `AuthController` endpoints are explicitly `[AllowAnonymous]`
- Input validation at multiple layers (FluentValidation + domain invariants)
- SQL injection prevention via EF Core parameterized queries
- CORS configuration for web clients

### Business Logic Security
- Domain-driven validation rules
- Aggregate boundaries prevent invalid state
- Immutable value objects prevent tampering
- Game ownership enforced at the application service layer (not just controller)

## Deployment Architecture

### Development
- Local development with file-based storage
- Docker support for consistent environments
- Swagger UI for API testing

### Production (Planned)
- Container orchestration (Kubernetes/Docker Swarm)
- Database persistence (SQL Server/PostgreSQL)
- Load balancing and scaling
- Monitoring and logging integration

## Future Enhancements

### Technical Improvements
- **MediatR**: CQRS pattern implementation ✅ Implemented
- **FluentValidation**: Enhanced input validation ✅ Implemented
- **Entity Framework Core 10**: Data persistence ✅ Implemented
- **Serilog**: Structured logging
- **JWT Authentication / Keycloak**: User security ✅ Implemented (identity façade over Keycloak 26)

### Feature Enhancements
- **Multiplayer Support**: Real-time gameplay
- **AI Opponents**: Computer player implementation ✅ Implemented (Random + Semantic Kernel strategies)
- **Game Statistics**: Player performance tracking
- **Tournament Mode**: Multi-game competitions

### Opponent Architecture (Implemented)

The AI opponent system follows Clean Architecture principles:

**Application Layer Contracts**:
- `IComputerOpponent`: Defines strategy interface with `SelectNextAttackAsync(Game, CancellationToken)` - always attacks Player's board
- `IComputerOpponentFactory`: Factory pattern for per-game strategy resolution
- `IPromptBuilder`: Abstracts LLM prompt construction
- `GameStateContext`: Simplified read-only record with essential game state (built internally by strategy)

**Infrastructure Implementations**:
- `RandomAttackStrategy`: Simple random cell selection from Player's board
- `SemanticKernelStrategy`: LLM-powered strategic reasoning using Microsoft Semantic Kernel
- `OpponentStrategyFactory`: Uses .NET 8 keyed DI services for strategy resolution
- `BattleshipPromptBuilder`: Constructs strategic prompts for LLM

**Per-Game Strategy Selection**:
- `OpponentStrategy` enum stored on `Game` aggregate (None, Random, SemanticKernel)
- Strategy selected at game creation time via API
- Factory resolves correct strategy based on game configuration

## Conclusion

The Battleship Game architecture provides a solid foundation for a maintainable, testable, and extensible system. The Clean Architecture and DDD patterns ensure that business logic remains at the center, while technical concerns are properly separated and abstracted. This design supports both current requirements and future enhancements while maintaining code quality and developer productivity.
