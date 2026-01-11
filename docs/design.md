# Battleship Game - System Design

## Architecture Overview

This document outlines the system design for the Battleship game, following **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns.

## System Architecture

The application is structured in layers:

1. **Domain Layer** (`BattleshipGame/Domain`): Core business logic and domain models
2. **Application Layer** (`BattleshipGame/Application`): Application services and use cases
3. **Infrastructure Layer** (`BattleshipGame/Infrastructure`): Data access and external services
4. **Presentation Layer** (`BattleshipGame.WebAPI`): REST API controllers and DTOs

## Domain Model Class Diagram

```mermaid
classDiagram
    class Game {
        <<AggregateRoot>>
        +GameId Id
        +PlayerId PlayerId
        +int BoardSize
        +GameState State
        +BoardSide WinnerSide
        +DateTime CreatedAt
        +DateTime LastUpdatedAt
        +Game(PlayerId, int)
        +ShipId PlaceShip(BoardSide, ShipKind, ShipOrientation, string)
        +void Attack(BoardSide, string)
        +ShipKind GetShipKind(BoardSide, ShipId)
        +bool IsGameOver(BoardSide)
        +bool IsReady(BoardSide)
    }

    class Player {
        <<AggregateRoot>>
        +PlayerId Id
        +GameId ActiveGameId
        +List GameHistory
    }

    class Board {
        <<Entity>>
        +BoardId Id
        +List Cells
        +List Ships
        +bool IsReady
        +bool IsGameOver
        +Board(int)
        +ShipId PlaceShip(ShipKind, ShipOrientation, string)
        +void Attack(string)
    }

    class Ship {
        <<Entity>>
        +ShipId Id
        +ShipKind Kind
        +List Position
        +bool Sunk
        +Ship(ShipKind, List)
        +void Attack(string)
    }

    class Cell {
        <<ValueObject>>
        +char Letter
        +int Digit
        +string Code
        +ShipId ShipId
        +CellState State
        +Cell(char, int)
        +void Assign(ShipId)
        +void Attack()
    }

    class GameId {
        <<ValueObject>>
        +Guid Value
    }

    class PlayerId {
        <<ValueObject>>
        +Guid Value
    }

    class BoardId {
        <<ValueObject>>
        +Guid Value
    }

    class ShipId {
        <<ValueObject>>
        +Guid Value
    }

    class CellState {
        <<Enumeration>>
        Clear
        Occupied
        Hit
        Missed
        Sunk
    }

    class ShipKind {
        <<Enumeration>>
        None
        Destroyer
        Submarine
        Cruiser
        Battleship
        Carrier
    }

    class ShipOrientation {
        <<Enumeration>>
        Vertical
        Horizontal
    }

    class BoardSide {
        <<Enumeration>>
        None
        Player
        Opponent
    }

    class GameState {
        <<Enumeration>>
        New
        Ready
        Started
        GameOver
    }

    Game "1" --> "2" Board : owns
    Board "1" --> "*" Cell : contains
    Board "1" --> "*" Ship : contains
    Ship --> ShipKind : has type
    Cell --> CellState : has state
    Cell --> ShipId : may reference
    Game --> GameState : has state
```

## Application Layer Design

```mermaid
classDiagram
    %% Application Services
    class IGameplayService {
        <<Interface>>
        +Task~GameId~ StartNewGameAsync(PlayerId, int)
        +Task~ShipId~ PlaceShipAsync(GameId, BoardSide, ShipKind, ShipOrientation, string)
        +Task StartGameplayAsync(GameId)
        +Task~LastRoundResult~ PlayerAttackThenCounterAttackAsync(GameId, string)
        +Task EndGameAsync(GameId)
    }

    class GameplayService {
        +Task~GameId~ StartNewGameAsync(PlayerId, int)
        +Task~ShipId~ PlaceShipAsync(GameId, BoardSide, ShipKind, ShipOrientation, string)
        +Task StartGameplayAsync(GameId)
        +Task~LastRoundResult~ PlayerAttackThenCounterAttackAsync(GameId, string)
        +Task EndGameAsync(GameId)
    }

    class AttackResult {
        <<Record>>
        +string TargetCell
        +CellState CellState
        +GameState GameState
        +BoardSide WinnerSide
        +ShipKind? SunkShip
        +int? ShipSize
    }

    class LastRoundResult {
        <<Record>>
        +GameId GameId
        +string PlayerTargetCell
        +CellState PlayerAttackResult
        +ShipKind? PlayerSunkShip
        +string? OpponentTargetCell
        +CellState? OpponentAttackResult
        +ShipKind? OpponentSunkShip
        +GameState GameState
        +BoardSide WinnerSide
    }

    class IPlayerService {
        <<Interface>>
        +Task~PlayerId~ CreateAsync(string)
        +Task~GetPlayerQueryResult?~ GetByIdAsync(PlayerId)
        +Task~GetPlayerQueryResult?~ GetByUsernameAsync(string)
    }

    class PlayerService {
        +Task~PlayerId~ CreateAsync(string)
        +Task~GetPlayerQueryResult?~ GetByIdAsync(PlayerId)
        +Task~GetPlayerQueryResult?~ GetByUsernameAsync(string)
    }

    %% Repository Interfaces
    class IGameRepository {
        <<Interface>>
        +Task~Game?~ GetByIdAsync(GameId)
        +Task SaveAsync(Game)
        +Task DeleteAsync(GameId)
        +Task~IEnumerable~Game~~ GetAllAsync()
    }

    class IPlayerRepository {
        <<Interface>>
        +Task~Player?~ GetByIdAsync(PlayerId)
        +Task~PlayerId~ SaveAsync(Player)
        +Task~Player?~ GetByUsernameAsync(string)
        +Task~bool~ UsernameExistsAsync(string)
    }

    %% CQRS via MediatR
    class Commands {
        <<CQRS>>
        CreateGameCommand
        PlaceShipCommand
        PlayerAttackCommand
        OpponentAttackCommand
        StartGameCommand
        EndGameCommand
    }

    class Queries {
        <<CQRS>>
        GetGameQuery
        GetPlayerQuery
    }

    GameplayService ..|> IGameplayService
    PlayerService ..|> IPlayerService
    GameplayService ..> IGameRepository : depends on
    GameplayService ..> IMediator : sends Commands/Queries
    GameplayService ..> LastRoundResult : returns
    PlayerService ..> IPlayerRepository : depends on
    PlayerService ..> IMediator : sends Commands/Queries
    Commands ..> Game : operates on
    PlayerAttackCommand ..> AttackResult : returns
    OpponentAttackCommand ..> AttackResult : returns
    Queries ..> Game : retrieves
```

## Web API Layer Design

```mermaid
classDiagram
    %% Controllers
    class GamesController {
        <<ApiController>>
        +IActionResult StartNewGame(CreateGameRequest)
        +ActionResult~GetGameQueryResult~ GetGame(Guid)
        +ActionResult~Guid~ PlaceShip(Guid, PlaceShipRequest)
        +ActionResult~LastRoundResult~ Attack(Guid, AttackRequest)
        +ActionResult~GameStateResponse~ UpdateGameState(Guid, UpdateGameStateRequest)
        +ActionResult~GameStateResponse~ GetGameState(Guid)
    }

    %% DTOs and Models
    class CreateGameRequest {
        +Guid PlayerId
        +int? BoardSize
    }

    class PlaceShipRequest {
        +BoardSide Side
        +ShipKind ShipKind
        +ShipOrientation Orientation
        +string BowCode
    }

    class AttackRequest {
        +string Cell
    }

    class UpdateGameStateRequest {
        +GameState State
    }

    class GetGameQueryResult {
        +Guid GameId
        +Guid PlayerId
        +int BoardSize
        +GameState State
        +BoardSide WinnerSide
    }

    class GameStateResponse {
        +GameState State
        +BoardSide WinnerSide
    }

    class LastRoundResult {
        +GameId GameId
        +string PlayerTargetCell
        +CellState PlayerAttackResult
        +ShipKind? PlayerSunkShip
        +string? OpponentTargetCell
        +CellState? OpponentAttackResult
        +ShipKind? OpponentSunkShip
        +GameState GameState
        +BoardSide WinnerSide
    }

    GamesController ..> CreateGameRequest
    GamesController ..> PlaceShipRequest
    GamesController ..> AttackRequest
    GamesController ..> UpdateGameStateRequest
    GamesController ..> GetGameQueryResult
    GamesController ..> GameStateResponse
    GamesController ..> LastRoundResult
```

## Key Design Patterns

### Domain-Driven Design (DDD)
- **Aggregate Roots**: `Game` and `Player` manage consistency boundaries
- **Entities**: `Board` and `Ship` have identity and lifecycle
- **Value Objects**: `Cell` and strongly-typed IDs ensure immutability
- **Domain Events**: Enable decoupled communication

### Clean Architecture
- **Dependency Inversion**: Application layer depends on domain abstractions
- **Separation of Concerns**: Each layer has distinct responsibilities
- **Testability**: Domain logic isolated from infrastructure concerns

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Extensible through interfaces and inheritance
- **Liskov Substitution**: Base classes properly extended
- **Interface Segregation**: Focused, cohesive interfaces
- **Dependency Inversion**: Depend on abstractions, not concretions

## Configuration and Constraints

### Board Configuration
- **Default Size**: 10x10 grid
- **Maximum Size**: 26x26 grid (A-Z columns, 1-26 rows)
- **Ship Allowance**: Exactly 5 ships per board

### Ship Configuration
- **Destroyer**: 2 cells
- **Submarine**: 3 cells
- **Cruiser**: 3 cells
- **Battleship**: 4 cells
- **Carrier**: 5 cells

### Game Rules
- Ships must be placed in straight lines (no diagonal)
- No overlapping ship positions
- Cannot attack the same cell twice
- Game ends when all ships on one board are sunk

## Error Handling Strategy

The system employs structured error handling:
- **Domain Exceptions**: Business rule violations throw appropriate exceptions
- **Validation**: Input validation at multiple layers
- **Error Messages**: Centralized error message management
- **API Responses**: Proper HTTP status codes and problem details
