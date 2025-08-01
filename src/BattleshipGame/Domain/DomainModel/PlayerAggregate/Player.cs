using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Domain.DomainModel.PlayerAggregate;

/// <summary>
/// Represents the unique identifier for a player.
/// </summary>
/// <remarks>This type encapsulates a <see cref="Guid"/> value to uniquely identify a game entity. It inherits
/// from <see cref="EntityId"/> to provide additional context or functionality specific to entity
/// identification.</remarks>
/// <param name="Value"></param>
public record PlayerId(Guid Value) : EntityId(Value);

/// <summary>
/// Represents a Battleship game player, including their active game and gameplay history.
/// </summary>
/// <remarks>This class encapsulates the player's identity, their currently active game (if any),  and a history
/// of games they have participated in. It is immutable and cannot be inherited.</remarks>
public sealed class Player : AggregateRoot<PlayerId>
{
    public GameId? ActiveGameId { get; } = null;
    public IList<GameId> GameHistory { get; } = [];
}
