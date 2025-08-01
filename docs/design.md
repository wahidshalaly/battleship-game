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
        +Game(PlayerId, int)
        +ShipId AddShip(BoardSide, ShipKind, ShipOrientation, string)
        +void Attack(BoardSide, string)
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
        +ShipId AddShip(ShipKind, ShipOrientation, string)
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
    }

    class ShipKind {
        <<Enumeration>>
        Destroyer
        Cruiser
        Submarine
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
        Own
        Opp
    }

    class GameState {
        <<Enumeration>>
        None
        Started
        BoardsAreReady
        InProgress
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
    class IGameService {
        <<Interface>>
        +GameId StartGame(Guid, Guid)
    }

    class GameService {
        +GameId StartGame(Guid, Guid)
    }

    %% Repository Interfaces
    class IGameRepository {
        <<Interface>>
        +Task~Game?~ GetByIdAsync(GameId)
        +Task SaveAsync(Game)
        +Task DeleteAsync(GameId)
    }

    GameService ..|> IGameService
    GameService ..> IGameRepository : depends on
    GameService ..> Game : creates
```

## Web API Layer Design

```mermaid
classDiagram
    %% Controllers
    class GamesController {
        <<ApiController>>
        -Dictionary~Guid,Game~ _games$
        +ActionResult~GameModel~ CreateGame(CreateGameRequest)
        +ActionResult~GameModel~ GetGame(Guid)
        +ActionResult~Guid~ AddShip(Guid, AddShipRequest)
        +IActionResult Attack(Guid, AttackRequest)
        +ActionResult~GameStateModel~ GetGameState(Guid)
    }

    %% DTOs and Models
    class CreateGameRequest {
        +PlayerId PlayerId
        +int? BoardSize
    }

    class AddShipRequest {
        +BoardSide Side
        +ShipKind ShipKind
        +ShipOrientation Orientation
        +string Bow
    }

    class AttackRequest {
        +BoardSide Side
        +string Cell
    }

    class GameModel {
        +Guid Id
        +string State
        +int BoardSize
        +GameModel From(Game)$
    }

    class GameStateModel {
        +string State
        +Guid? Winner
    }

    GamesController ..> CreateGameRequest
    GamesController ..> AddShipRequest
    GamesController ..> AttackRequest
    GamesController ..> GameModel
    GamesController ..> GameStateModel
    GamesController ..> Game : creates/manages
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
