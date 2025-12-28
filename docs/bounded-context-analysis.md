# Battleship Game Bounded Context Analysis

## Overview

This document analyzes the Battleship Game Bounded Context diagram and compares it with the current codebase implementation to identify alignment, gaps, and opportunities for enhancement.

## Bounded Context Diagram Analysis

Based on the attached diagram, the Battleship Game bounded context includes:

### Core Domain Components

#### 1. Player Aggregate
**From Diagram:**
- Player entity with PlayerId
- GameHistory collection
- ActiveGameId reference

**Current Implementation:** ✅ **ALIGNED**
```csharp
public sealed class Player : AggregateRoot<PlayerId>
{
    public GameId? ActiveGameId { get; } = null;
    public IList<GameId> GameHistory { get; } = [];
}
```

#### 2. Game Aggregate
**From Diagram:**
- Game entity with GameId
- Board entities (internal)
- Ship entities with references to Cell
- Domain events: GameOver, CellAttacked, ShipSunk

**Current Implementation:** ✅ **MOSTLY ALIGNED**
```csharp
public sealed class Game : AggregateRoot<GameId>
{
    private readonly Board _ownBoard;
    private readonly Board _oppBoard;
    public PlayerId PlayerId { get; init; }
    public int BoardSize { get; init; }
    public GameState State { get; private set; }
}
```

**Gap Identified:** ❌ Domain events not implemented yet

### Domain Events (From Diagram)

#### Expected Events:
1. **GameOver** - When game completes
2. **CellAttacked** - When a cell is attacked
3. **ShipSunk** - When a ship is destroyed

#### Current Implementation:
- ✅ Domain event infrastructure exists (`IDomainEvent`, `DomainEvent<T>`)
- ❌ Specific game events not yet implemented
- ❌ Event publishing not integrated into game logic

### API Endpoints Analysis

#### From Diagram:
**Player API:**
- `POST /api/players/` - Create new player
- `GET /api/players/{id}` - Get player details
- `GET /api/players/{id}/active_game` - Get active game
- `GET /api/players/{id}/game_history` - Get game history

**Game API:**
- `POST /api/games/{id}/attack` - Attack cell
- `GET /api/games/{id}/status` - Get game status
- `POST /api/games/{id}/ships` - Add ship

#### Current Implementation:
**Games Controller:** ✅ **PARTIALLY ALIGNED**
- ✅ `POST /api/games` - Create game
- ✅ `GET /api/games/{id}` - Get game
- ✅ `POST /api/games/{id}/ships` - Add ship
- ✅ `POST /api/games/{id}/attack` - Attack cell
- ✅ `GET /api/games/{id}/state` - Get game state

**Missing:** ❌ Player API completely missing

### User Stories (From Diagram)

#### Player Aggregate Stories:
1. **Sign up a new player** - ❌ Not implemented
2. **Sign in an existing player** - ❌ Not implemented
3. **Start a new game** - ❌ Partially (no player context)
4. **Retrieve player's history** - ❌ Not implemented

#### Game Aggregate Stories:
1. **Start a new game** - ✅ Implemented
2. **Place ships on boards** - ✅ Implemented
3. **Take turns on attack** - ✅ Implemented
4. **Announce a winner** - ✅ Partially (state tracking)

## Alignment Assessment

### ✅ Strong Alignments

1. **Aggregate Design**: Both Player and Game aggregates match the diagram
2. **Entity Structure**: Board, Ship, Cell entities correctly implemented
3. **Value Objects**: Strongly-typed IDs and coordinate system
4. **Core Game Logic**: Ship placement and attack mechanics work as designed
5. **Game State Management**: Proper state transitions implemented

### ⚠️ Partial Alignments

1. **API Coverage**: Game API mostly complete, Player API missing
2. **Domain Events**: Infrastructure ready but events not implemented
3. **Player Management**: Entity exists but no controller or business logic

### ❌ Key Gaps

1. **Domain Events**: No concrete events (GameOver, CellAttacked, ShipSunk)
2. **Player API**: Complete Player management API missing
3. **Authentication/Sessions**: No player authentication system
4. **Cross-Aggregate Operations**: Player-Game relationships not enforced
5. **Event Sourcing**: No event persistence or replay capabilities

## Recommendations for Implementation

### Phase 1: Domain Events Implementation

```csharp
// Missing domain events to implement
public class GameOverEvent : DomainEvent<GameOverEvent>
{
    public GameId GameId { get; init; }
    public PlayerId WinnerId { get; init; }
    public PlayerId LoserId { get; init; }
    public DateTime GameEndTime { get; init; }
}

public class CellAttackedEvent : DomainEvent<CellAttackedEvent>
{
    public GameId GameId { get; init; }
    public PlayerId AttackerId { get; init; }
    public string CellCode { get; init; }
    public bool IsHit { get; init; }
    public bool IsShipSunk { get; init; }
}

public class ShipSunkEvent : DomainEvent<ShipSunkEvent>
{
    public GameId GameId { get; init; }
    public ShipId ShipId { get; init; }
    public ShipKind ShipKind { get; init; }
    public PlayerId OwnerId { get; init; }
}
```

### Phase 2: Player API Implementation

```csharp
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    // POST /api/players
    public ActionResult<PlayerModel> CreatePlayer([FromBody] CreatePlayerRequest request)

    // GET /api/players/{id}
    public ActionResult<PlayerModel> GetPlayer([FromRoute] Guid id)

    // GET /api/players/{id}/active_game
    public ActionResult<GameModel> GetActiveGame([FromRoute] Guid id)

    // GET /api/players/{id}/game_history
    public ActionResult<List<GameModel>> GetGameHistory([FromRoute] Guid id)
}
```

### Phase 3: Enhanced Game Logic

1. **Event Publishing**: Integrate domain events into game operations
2. **Cross-Aggregate Validation**: Ensure player-game consistency
3. **Game History Management**: Automatically track completed games
4. **Turn Management**: Implement proper turn-based mechanics

### Phase 4: Advanced Features

1. **Event Sourcing**: Store and replay domain events
2. **CQRS Implementation**: Separate read/write models
3. **Real-time Updates**: SignalR for live game updates
4. **Player Statistics**: Aggregate game performance data

## Implementation Priority Matrix

| Feature | Business Impact | Implementation Effort | Priority |
|---------|----------------|----------------------|----------|
| Domain Events | High | Low | 🔴 Critical |
| Player API | High | Medium | 🔴 Critical |
| Event Publishing | Medium | Low | 🟡 Important |
| Player Authentication | Medium | High | 🟡 Important |
| Cross-Aggregate Validation | High | Medium | 🟡 Important |
| Game History Tracking | Low | Low | 🟢 Nice to Have |
| Real-time Updates | Low | High | 🟢 Nice to Have |

## Architecture Enhancement Suggestions

### 1. Domain Events Integration

```csharp
// In Game aggregate
public void Attack(BoardSide boardSide, string cell)
{
    var board = BoardSelector(boardSide);
    var wasHit = board.Attack(cell);

    // Raise domain event
    AddDomainEvent(new CellAttackedEvent
    {
        GameId = Id,
        AttackerId = PlayerId,
        CellCode = cell,
        IsHit = wasHit,
        IsShipSunk = board.WasShipSunk(cell)
    });

    if (IsGameOver(boardSide))
    {
        State = GameState.GameOver;
        AddDomainEvent(new GameOverEvent
        {
            GameId = Id,
            WinnerId = GetWinnerId(),
            LoserId = GetLoserId(),
            GameEndTime = DateTime.UtcNow
        });
    }
}
```

### 2. Enhanced Player Management

```csharp
public sealed class Player : AggregateRoot<PlayerId>
{
    public string Username { get; private set; }
    public GameId? ActiveGameId { get; private set; }
    public IList<GameId> GameHistory { get; private set; }
    public PlayerStatistics Statistics { get; private set; }

    public void StartGame(GameId gameId)
    {
        if (ActiveGameId != null)
            throw new InvalidOperationException("Player already has an active game");

        ActiveGameId = gameId;
        AddDomainEvent(new PlayerGameStartedEvent { PlayerId = Id, GameId = gameId });
    }

    public void CompleteGame(GameId gameId, bool won)
    {
        if (ActiveGameId != gameId)
            throw new InvalidOperationException("Game is not the active game");

        ActiveGameId = null;
        GameHistory.Add(gameId);
        Statistics.UpdateWithGameResult(won);

        AddDomainEvent(new PlayerGameCompletedEvent
        {
            PlayerId = Id,
            GameId = gameId,
            Won = won
        });
    }
}
```

### 3. CQRS Implementation

```csharp
// Commands
public record StartGameCommand(PlayerId PlayerId, int BoardSize);
public record PlaceShipCommand(GameId GameId, BoardSide Side, ShipKind Kind, ShipOrientation Orientation, string Bow);
public record AttackCommand(GameId GameId, BoardSide Side, string Cell);

// Queries
public record GetGameQuery(GameId GameId);
public record GetPlayerGamesQuery(PlayerId PlayerId);
public record GetGameHistoryQuery(PlayerId PlayerId);

// Handlers using MediatR
public class StartGameCommandHandler : IRequestHandler<StartGameCommand, GameId>
public class AttackCommandHandler : IRequestHandler<AttackCommand>
public class GetGameQueryHandler : IRequestHandler<GetGameQuery, GameModel>
```

## Conclusion

The current implementation shows strong alignment with the core domain model from the Bounded Context diagram, particularly in aggregate design and entity relationships. However, significant gaps exist in:

1. **Domain Events**: Critical missing piece for proper DDD implementation
2. **Player API**: Essential for complete user story coverage
3. **Cross-Aggregate Coordination**: Needed for data consistency

The recommended implementation phases will bring the codebase into full alignment with the Bounded Context vision while maintaining clean architecture principles and enabling future enhancements like event sourcing and real-time features.

**Next Steps:**
1. Implement the three core domain events (GameOver, CellAttacked, ShipSunk)
2. Create the Player API controller with full CRUD operations
3. Integrate event publishing into the Game aggregate methods
4. Add cross-aggregate validation for player-game relationships
