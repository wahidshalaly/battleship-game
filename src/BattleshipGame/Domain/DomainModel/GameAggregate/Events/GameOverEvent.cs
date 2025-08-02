using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when a game is finished.
/// </summary>
public class GameOverEvent : DomainEvent<GameOverEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; }

    /// <summary>
    /// Gets the winner's player identifier.
    /// </summary>
    public PlayerId WinnerId { get; }

    /// <summary>
    /// Gets the loser's player identifier.
    /// </summary>
    public PlayerId LoserId { get; }

    /// <summary>
    /// Gets the timestamp when the game ended.
    /// </summary>
    public DateTimeOffset EndedAt { get; }

    /// <summary>
    /// Initializes a new instance of the GameOverEvent class.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="winnerId">The winner's player identifier.</param>
    /// <param name="loserId">The loser's player identifier.</param>
    /// <param name="endedAt">The timestamp when the game ended.</param>
    public GameOverEvent(GameId gameId, PlayerId winnerId, PlayerId loserId, DateTimeOffset endedAt)
    {
        GameId = gameId;
        WinnerId = winnerId;
        LoserId = loserId;
        EndedAt = endedAt;
    }
}
