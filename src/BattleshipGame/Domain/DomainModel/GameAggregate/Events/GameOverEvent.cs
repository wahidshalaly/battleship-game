using BattleshipGame.Domain.DomainModel.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when a game is finished.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameOverEvent class.
/// </remarks>
/// <param name="gameId">The game identifier.</param>
/// <param name="winnerSide">The winner's player identifier.</param>
public class GameOverEvent(GameId gameId, BoardSide winnerSide) : DomainEvent<GameOverEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;

    /// <summary>
    /// Gets the winner's board side.
    /// </summary>
    public BoardSide WinnerSide { get; } = winnerSide;
}
