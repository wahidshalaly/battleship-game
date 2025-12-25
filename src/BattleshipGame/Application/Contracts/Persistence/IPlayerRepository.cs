using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Contracts.Persistence;

/// <summary>
/// Repository contract for Player aggregate persistence operations.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Retrieves a player by their identifier.
    /// </summary>
    /// <param name="playerId">The player identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The player if found, otherwise null.</returns>
    Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken ct);

    /// <summary>
    /// Saves a new or existing player.
    /// </summary>
    /// <param name="player">The player to save.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The saved player identifier.</returns>
    Task<PlayerId> SaveAsync(Player player, CancellationToken ct);

    /// <summary>
    /// Finds a player by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The player if found, otherwise null.</returns>
    Task<Player?> GetByUsernameAsync(string username, CancellationToken ct);

    /// <summary>
    /// Checks if a username is already taken.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if username exists, otherwise false.</returns>
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
}
