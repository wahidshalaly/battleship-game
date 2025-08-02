using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when a cell is attacked during gameplay.
/// </summary>
public class CellAttackedEvent : DomainEvent<CellAttackedEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; }

    /// <summary>
    /// Gets the attacking player's identifier.
    /// </summary>
    public PlayerId AttackerId { get; }

    /// <summary>
    /// Gets the defending player's identifier.
    /// </summary>
    public PlayerId DefenderId { get; }

    /// <summary>
    /// Gets the attacked cell coordinate.
    /// </summary>
    public string CellCoordinate { get; }

    /// <summary>
    /// Gets a value indicating whether the attack was a hit.
    /// </summary>
    public bool IsHit { get; }

    /// <summary>
    /// Gets the ship identifier if a ship was hit, otherwise null.
    /// </summary>
    public ShipId? HitShipId { get; }

    /// <summary>
    /// Gets the timestamp when the attack occurred.
    /// </summary>
    public DateTimeOffset AttackedAt { get; }

    /// <summary>
    /// Initializes a new instance of the CellAttackedEvent class.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="attackerId">The attacking player's identifier.</param>
    /// <param name="defenderId">The defending player's identifier.</param>
    /// <param name="cellCoordinate">The attacked cell coordinate.</param>
    /// <param name="isHit">Whether the attack was a hit.</param>
    /// <param name="hitShipId">The ship identifier if a ship was hit.</param>
    /// <param name="attackedAt">The timestamp when the attack occurred.</param>
    public CellAttackedEvent(
        GameId gameId,
        PlayerId attackerId,
        PlayerId defenderId,
        string cellCoordinate,
        bool isHit,
        ShipId? hitShipId = null,
        DateTimeOffset? attackedAt = null)
    {
        GameId = gameId;
        AttackerId = attackerId;
        DefenderId = defenderId;
        CellCoordinate = cellCoordinate;
        IsHit = isHit;
        HitShipId = hitShipId;
        AttackedAt = attackedAt ?? DateTimeOffset.UtcNow;
    }
}
