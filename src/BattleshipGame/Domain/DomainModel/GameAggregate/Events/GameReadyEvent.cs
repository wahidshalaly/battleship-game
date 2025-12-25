using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when both boards in a game are ready for gameplay.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameReadyEvent class.
/// </remarks>
/// <param name="gameId">The game identifier.</param>
public class GameReadyEvent(GameId gameId) : DomainEvent<GameReadyEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;
}
