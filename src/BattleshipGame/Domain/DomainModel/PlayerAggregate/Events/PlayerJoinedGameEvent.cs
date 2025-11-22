using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;

/// <summary>
/// Domain event raised when a player joins a game.
/// </summary>
/// <remarks>
/// Initializes a new instance of the PlayerJoinedGameEvent class.
/// </remarks>
/// <param name="playerId">The player identifier.</param>
/// <param name="gameId">The game identifier.</param>
/// <param name="username">The player's username.</param>
public class PlayerJoinedGameEvent(PlayerId playerId, GameId gameId, string username)
    : DomainEvent<PlayerJoinedGameEvent>
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
