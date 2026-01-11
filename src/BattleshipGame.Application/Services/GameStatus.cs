using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Services;

/// <summary>
/// Represents the result of an individual attack in the game.
/// </summary>
/// <param name="TargetCell">The cell code that was attacked (e.g., "A5").</param>
/// <param name="CellState">The state of the attacked cell (Hit, Miss, Sunk).</param>
/// <param name="GameState">The current state of the game after the attack.</param>
/// <param name="WinnerSide">The winning side if the game is over, otherwise None.</param>
/// <param name="SunkShip">The kind of ship that was sunk, if applicable.</param>
public record AttackResult(
    string TargetCell,
    CellState CellState,
    GameState GameState,
    BoardSide WinnerSide,
    ShipKind? SunkShip,
    int? ShipSize
);

/// <summary>
/// Represents the complete outcome of a game round including both player and opponent attacks.
/// </summary>
/// <param name="GameId">The game identifier.</param>
/// <param name="PlayerTargetCell">The cell code attacked by the player.</param>
/// <param name="PlayerAttackResult">The result of the player's attack.</param>
/// <param name="PlayerSunkShip">The opponent's ship that was sunk by player, if any.</param>
/// <param name="OpponentTargetCell">The cell code attacked by the opponent (null if game ended after player's attack).</param>
/// <param name="OpponentAttackResult">The result of the opponent's attack (null if game ended after player's attack).</param>
/// <param name="OpponentSunkShip">The player's ship that was sunk by opponent, if any.</param>
/// <param name="GameState">The current state of the game after the round.</param>
/// <param name="WinnerSide">The winning side if the game is over, otherwise None.</param>
public record LastRoundResult(
    GameId GameId,
    string PlayerTargetCell,
    CellState PlayerAttackResult,
    ShipKind? PlayerSunkShip,
    string? OpponentTargetCell,
    CellState? OpponentAttackResult,
    ShipKind? OpponentSunkShip,
    GameState GameState,
    BoardSide WinnerSide
);
