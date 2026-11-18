# Battleship Game — Bounded Context Analysis

## Purpose
Provide a concise analysis of the Battleship Game bounded contexts, their responsibilities, boundaries, contracts, and interactions, aligned with the domain diagram.

![Battleship Game Bounded Context](Battleship_Game.png)

## Context Map

- **Game Context**
  - Responsibility: Gameplay lifecycle (setup, start, turns, win/lose)
  - Core model: `Game` aggregate with `Board`, `Ship`, `Cell`
  - Events: `GameReady`, `GameStarted`, `UnderAttack`, `ShipSunk`, `GameOver`
  - External contract: Games API — create, place ship, attack (returns `LastRoundResult`), get/update state

- **Player Context**
  - Responsibility: Player identity
  - Core model: `Player` aggregate (`Username`, `IdentitySubject`)
  - A player's active game and history are derived from the Game Context (games reference their owner by `PlayerId`); the player stores no copy, so there is nothing to keep in sync on `GameOver`
  - External contract: Players API — create, get by ID, get by username (history endpoint pending)

- **Shared Kernel**
  - Purpose: Common, stable types shared across contexts
  - Types: Strongly-typed IDs (`GameId`, `PlayerId`, `ShipId`), enums (`GameState`, `BoardSide`, `CellState`, `ShipKind`, `ShipOrientation`)
  - Rule: Only pure abstractions and primitives; no business logic

## Boundaries & Ownership

- **Ownership**
  - Game Context owns board/ship state, attack rules, and game state transitions
  - Player Context owns player identity only; which game is active (not over) is decided by the Game Context

- **Integration**
  - Domain events are the integration mechanism between contexts
  - Aggregates reference each other by ID in one direction only (`Game` → `Player`), so no event is needed to keep a back-reference in sync

## Contracts (External APIs)

- **Games API** (type-safe enums)
  - `POST /api/games` — create game
  - `GET /api/games/{id}` — retrieve game (includes `GameState`, `WinnerSide`)
  - `POST /api/games/{id}/ships` — place ship (`BoardSide`, `ShipKind`, `ShipOrientation`)
  - `POST /api/games/{id}/attacks` — player attack + counter-attack → returns `LastRoundResult`
  - `PUT /api/games/{id}/state`, `GET /api/games/{id}/state` — update/get state

- **Players API**
  - `POST /api/players` — create player
  - `GET /api/players/{id}` — get player by ID
  - `GET /api/players/{username}` — get player by username
  - (Planned) `GET /api/players/{id}/game_history` — expose history

## Invariants (Per Context)

- **Game Context**
  - Valid transitions: New → Ready → Started → GameOver
  - Attack preconditions: Not GameOver, Started, correct `TargetSide`
  - Board constraints: no overlaps, valid placement, 5 ships limit

- **Player Context**
  - At most one active game (not `GameOver`) per player: checked by `StartNewGame` and enforced by a filtered unique index on `games.player_id`

## Interactions

- Commands in Game Context produce domain events; rich results (`AttackResult`, `LastRoundResult`) returned to clients
- Player-facing views (active game, games played) are queries over games

## Observations (Alignment)

- Contexts and boundaries match the diagram
- Shared kernel centralizes common types correctly
- Games API fully reflects Game Context contract and returns rich, context-appropriate results
- Players API is implemented (create/get); history exposure is pending

## Next Steps (Context-Focused)

- Player statistics (games won, win rate, leaderboard) are queries over finished games, not state copied onto the player
- Player-owned facts that cannot be derived from games (achievements, points ledger, rating) would be new aggregates keyed by `PlayerId`, recorded by handlers reacting to `GameOver`

This analysis focuses on bounded contexts, responsibilities, and contracts. Implementation work is tracked in GitHub issues.