using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when both boards in a game are ready for gameplay.
/// </summary>
/// <remarks>
/// Initializes a new instance of the BoardsReadyEvent class.
/// </remarks>
/// <param name="gameId">The game identifier.</param>
public class BoardsReadyEvent(GameId gameId) : DomainEvent<BoardsReadyEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;
}
