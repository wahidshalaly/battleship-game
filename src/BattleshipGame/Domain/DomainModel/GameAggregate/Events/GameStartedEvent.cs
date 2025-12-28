using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when gameplay starts.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameStartedEvent class.
/// </remarks>
/// <param name="gameId">The game identifier.</param>
public class GameStartedEvent(GameId gameId) : DomainEvent<GameStartedEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;
}
