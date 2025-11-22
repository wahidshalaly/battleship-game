using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;

/// <summary>
/// Domain event raised when a player leaves a game.
/// </summary>
/// <remarks>
/// Initializes a new instance of the PlayerLeftGameEvent class.
/// </remarks>
/// <param name="playerId">The player identifier.</param>
/// <param name="gameId">The game identifier.</param>
/// <param name="username">The player's username.</param>
public class PlayerLeftGameEvent(PlayerId playerId, GameId gameId, string username)
    : DomainEvent<PlayerLeftGameEvent>
{
    /// <summary>
    /// Gets the player identifier.
    /// </summary>
    public PlayerId PlayerId { get; } = playerId;

    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId GameId { get; } = gameId;

    /// <summary>
    /// Gets the player's username.
    /// </summary>
    public string Username { get; } = username;
}
