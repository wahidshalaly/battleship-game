using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Services;

/// <summary>
/// Orchestrates gameplay lifecycle: setup, ship placement, attacks, termination.
/// Coordinates aggregates without embedding domain logic.
/// </summary>
public interface IGameplayService
{
    /// <summary>
    /// Starts a new game for the specified player with the given board size.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="boardSize"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<GameId> StartNewGameAsync(
        PlayerId playerId,
        int boardSize,
        OpponentStrategy opponentStrategy,
        CancellationToken ct
    );

    /// <summary>
    /// Places a ship on the specified side of the board.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="side"></param>
    /// <param name="kind"></param>
    /// <param name="orientation"></param>
    /// <param name="bowCode"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ShipId> PlaceShipAsync(
        GameId gameId,
        BoardSide side,
        ShipKind kind,
        ShipOrientation orientation,
        string bowCode,
        CancellationToken ct
    );

    /// <summary>
    /// Executes a player attack followed by an opponent counter-attack.
    /// Returns the complete outcome of the round including both attacks.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="cellCode">The cell code for the player to attack.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The complete round result with both player and opponent attack outcomes.</returns>
    Task<LastRoundResult> PlayerAttackThenCounterAttackAsync(
        GameId gameId,
        string cellCode,
        CancellationToken ct
    );

    /// <summary>
    /// Ends the specified game.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task EndGameAsync(GameId gameId, CancellationToken ct);
}
