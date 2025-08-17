using BattleshipGame.Domain.DomainModel.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when a ship is completely sunk.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ShipSunkEvent class.
/// </remarks>
/// <param name="gameId">The game identifier.</param>
/// <param name="shipId">The sunk ship's identifier.</param>
/// <param name="attackedSide">The side that was attacked (owns the sunk ship).</param>
public class ShipSunkEvent(
    GameId gameId,
    ShipId shipId,
    BoardSide attackedSide) : DomainEvent<ShipSunkEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;

    /// <summary>
    /// Gets the sunk ship's identifier.
    /// </summary>
    public ShipId ShipId { get; } = shipId;

    /// <summary>
    /// Gets the side that was attacked (owns the sunk ship).
    /// </summary>
    public BoardSide AttackedSide { get; } = attackedSide;
}
