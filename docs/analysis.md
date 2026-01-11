# Battleship Game - Domain Analysis

## Overview

This document provides a comprehensive analysis of the Battleship game domain model, detailing the core entities, value objects, and business rules that govern the game logic.

## Architecture

The system follows **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns:
- **Domain Layer**: Contains core business logic, entities, value objects, and domain events
- **Application Layer**: Contains application services and use cases
- **Infrastructure Layer**: Contains repositories and external integrations
- **Presentation Layer**: Contains Web API controllers and DTOs

## Domain Model Components

### Core Entities and Aggregates

#### Game Aggregate Root
The `Game` entity serves as the aggregate root and orchestrates the entire game lifecycle:
- **GameId**: Strongly-typed identifier using `EntityId<Guid>`
- **PlayerId**: Associated player identifier
- **BoardSize**: Configurable board dimensions (10-26)
- **GameState**: Current game state enumeration
- **TargetSide**: Which board side is the target for the next attack
- **Boards**: Two internal boards (Own and Opponent)

**Business Rules:**
- Game starts in `New` state
- Supports configurable board sizes from 10x10 to 26x26
- Manages two separate boards for own and opponent sides
- Enforces turn-based gameplay through `TargetSide` property
- Player always attacks first (Opponent board is initial target)
- Turn alternates after each attack

#### Board Entity
Internal entity that manages the game field and ship placement:
- **BoardId**: Strongly-typed identifier
- **Cells**: Dictionary of cells indexed by coordinate codes
- **Ships**: Collection of placed ships (maximum 5)
- **IsReady**: Indicates if all ships are placed
- **IsGameOver**: Indicates if all ships are sunk

**Business Rules:**
- Board size ranges from 10x10 (default) to 26x26 (maximum)
- Exactly 5 ships must be placed (one of each kind)
- Ship placement validation ensures no overlaps or invalid positions
- Attack validation prevents attacking the same cell twice

#### Ship Entity
Represents individual ships on the board:
- **ShipId**: Strongly-typed identifier
- **ShipKind**: Enumeration defining ship type and size
- **Position**: Collection of occupied cell coordinates
- **Sunk**: Calculated property based on hits vs. position

**Business Rules:**
- Ship sizes: Destroyer(2), Submarine(3), Cruiser(3), Battleship(4), Carrier(5)
- Ships must be placed in straight lines (horizontal or vertical)
- Ships sink when all occupied cells are hit
- Each ship can only be placed once per board

#### Player Aggregate Root
Represents a game participant:
- **PlayerId**: Strongly-typed identifier
- **ActiveGameId**: Current game reference
- **GameHistory**: Collection of previous games

### Value Objects

#### Cell Value Object
Represents individual board positions:
- **Letter**: Column identifier (A-Z)
- **Digit**: Row identifier (1-26)
- **Code**: Computed coordinate string (e.g., "D4")
- **ShipId**: Reference to occupying ship (nullable)
- **CellState**: Current state enumeration

**Business Rules:**
- Immutable coordinate representation
- State transitions: Clear → Occupied → Hit
- Prevents reassignment once occupied
- Prevents re-attacking hit cells

#### Strongly-Typed Identifiers
All entities use strongly-typed identifiers inheriting from `EntityId`:
- **GameId(Guid)**: Game identifier
- **BoardId(Guid)**: Board identifier
- **ShipId(Guid)**: Ship identifier
- **PlayerId(Guid)**: Player identifier

### Enumerations

#### CellState
- **Clear**: Unoccupied and not attacked
- **Occupied**: Contains a ship
- **Hit**: Has been attacked (occupied cell targeted)
- **Missed**: Has been attacked (empty cell targeted)

#### ShipKind with Sizes
- **Destroyer**: 2 cells
- **Submarine**: 3 cells
- **Cruiser**: 3 cells
- **Battleship**: 4 cells
- **Carrier**: 5 cells

#### ShipOrientation
- **Vertical**: Ships placed vertically
- **Horizontal**: Ships placed horizontally

#### BoardSide
- **Player**: Player's own board
- **Opponent**: Opponent's board

#### GameState
- **None**: Initial state
- **New**: Game created, setup required
- **Ready**: All ships placed
- **Started**: Active gameplay
- **GameOver**: Game completed

## Domain Events

The system uses the **Domain Events** pattern for decoupled communication:
- Base classes: `IDomainEvent`, `DomainEvent`, `IDomainEventHandler`
- Events are raised through the aggregate root pattern
- Enables cross-cutting concerns like logging and notifications

## Key Business Rules

### Ship Placement Rules
1. Exactly 5 ships per board (one of each kind)
2. Ships must be placed in straight lines only
3. No overlapping ship positions
4. Ships must fit within board boundaries
5. Each ship kind can only be placed once

### Turn Control Rules
1. **TargetSide**: Tracks which board side is allowed to be attacked next
2. **Initial Turn**: When gameplay starts, `TargetSide` is set to `Opponent` (Player always attacks first)
3. **Turn Alternation**: After each attack, `TargetSide` switches to the opposite side
4. **Turn Validation**: `Attack()` method validates that the provided `boardSide` matches `TargetSide`
5. **Invalid Turn**: Throws `InvalidTargetSideException` if attacking the wrong board
6. **Turn Progression**: 
   - Player attacks Opponent board → `TargetSide` becomes `Player`
   - Opponent attacks Player board → `TargetSide` becomes `Opponent`
7. **Game Over**: Turn control ends when `GameState` becomes `GameOver`

### Attack Rules
1. Must attack the board specified by `TargetSide` property
2. Cannot attack the same cell twice
3. Attacks must target valid board coordinates
4. Hits on occupied cells damage the ship
5. Ships sink when all cells are hit
6. Turn automatically switches to opponent after each attack

### Game Flow Rules
1. Game starts in `New` state
2. Players place ships during setup phase
3. Game becomes `Started` when both boards are ready via `StartGameplay()`
4. `TargetSide` is initialized to `Opponent` (Player's first turn)
5. Players alternate turns by attacking the board specified by `TargetSide`
6. Game ends when all ships on one board are sunk
7. Turn-based gameplay enforced through `TargetSide` validation

## Error Handling

The domain uses structured error handling:
- **ArgumentException**: Invalid input parameters
- **ApplicationException**: Business rule violations
- **InvalidTargetSideException**: Thrown when attacking wrong board (not matching `TargetSide`)
- **GameNotReadyException**: Thrown when attempting to start gameplay before boards are ready
- **Custom error messages**: Centralized in `ErrorMessages` class

## Testing Strategy

The domain follows **Test-Driven Development (TDD)** principles:
- **Unit Tests**: Cover all entities, value objects, and business rules
- **FluentAssertions**: Readable test assertions
- **xUnit**: Primary testing framework
- **Test Coverage**: Comprehensive coverage of domain logic
