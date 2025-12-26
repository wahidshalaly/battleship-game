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
    Task<GameId> StartNewGameAsync(PlayerId playerId, int boardSize, CancellationToken ct);

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
    /// Executes an attack on the specified side of the board at the given cell.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="side"></param>
    /// <param name="cellCode"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<GameStatus> PlayerAttackAndCounterAttackAsync(
        GameId gameId,
        string cellCode,
        CancellationToken ct
    );

    /// <summary>
    /// Starts the gameplay loop for the specified game.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task StartGameplayAsync(GameId gameId, CancellationToken ct);

    /// <summary>
    /// Checks the current status of the game.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<GameStatus> CheckGameStatusAsync(GameId gameId, CancellationToken ct);

    /// <summary>
    /// Ends the specified game.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task EndGameAsync(GameId gameId, CancellationToken ct);
}
