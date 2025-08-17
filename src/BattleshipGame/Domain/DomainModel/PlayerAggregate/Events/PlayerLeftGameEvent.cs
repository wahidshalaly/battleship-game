using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;

/// <summary>
/// Domain event raised when a player leaves a game.
/// </summary>
public class PlayerLeftGameEvent : DomainEvent<PlayerLeftGameEvent>
{
    /// <summary>
    /// Gets the player identifier.
    /// </summary>
    public PlayerId PlayerId { get; }

    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; }

    /// <summary>
    /// Gets the player's username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Initializes a new instance of the PlayerLeftGameEvent class.
    /// </summary>
    /// <param name="playerId">The player identifier.</param>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="username">The player's username.</param>
    public PlayerLeftGameEvent(PlayerId playerId, GameId gameId, string username)
    {
        PlayerId = playerId;
        GameId = gameId;
        Username = username;
    }
}
