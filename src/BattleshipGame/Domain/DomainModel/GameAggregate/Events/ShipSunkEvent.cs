using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when a ship is completely sunk.
/// </summary>
public class ShipSunkEvent : DomainEvent<ShipSunkEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; }

    /// <summary>
    /// Gets the sunk ship's identifier.
    /// </summary>
    public ShipId ShipId { get; }

    /// <summary>
    /// Gets the kind of ship that was sunk.
    /// </summary>
    public ShipKind ShipKind { get; }

    /// <summary>
    /// Gets the owner's player identifier.
    /// </summary>
    public PlayerId OwnerId { get; }

    /// <summary>
    /// Gets the attacking player's identifier.
    /// </summary>
    public PlayerId AttackerId { get; }

    /// <summary>
    /// Gets the timestamp when the ship was sunk.
    /// </summary>
    public DateTimeOffset SunkAt { get; }

    /// <summary>
    /// Initializes a new instance of the ShipSunkEvent class.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="shipId">The sunk ship's identifier.</param>
    /// <param name="shipKind">The kind of ship that was sunk.</param>
    /// <param name="ownerId">The owner's player identifier.</param>
    /// <param name="attackerId">The attacking player's identifier.</param>
    /// <param name="sunkAt">The timestamp when the ship was sunk.</param>
    public ShipSunkEvent(
        GameId gameId,
        ShipId shipId,
        ShipKind shipKind,
        PlayerId ownerId,
        PlayerId attackerId,
        DateTimeOffset? sunkAt = null)
    {
        GameId = gameId;
        ShipId = shipId;
        ShipKind = shipKind;
        OwnerId = ownerId;
        AttackerId = attackerId;
        SunkAt = sunkAt ?? DateTimeOffset.UtcNow;
    }
}
