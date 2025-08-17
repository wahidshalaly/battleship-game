using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Contracts.Persistence;

/// <summary>
/// Repository contract for Game aggregate persistence operations.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Retrieves a game by its identifier.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The game if found, otherwise null.</returns>
    Task<Game?> GetByIdAsync(GameId gameId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a new or existing game.
    /// </summary>
    /// <param name="game">The game to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved game identifier.</returns>
    Task SaveAsync(Game game, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a game by its identifier.
    /// </summary>
    /// <param name="gameId">The game identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(GameId gameId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all games for a specific player.
    /// </summary>
    /// <param name="playerId">The player identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of games for the player.</returns>
    Task<IReadOnlyCollection<Game>> GetByPlayerIdAsync(PlayerId playerId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the active game for a specific player, if any.
    /// </summary>
    /// <param name="playerId">The player identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active game if found, otherwise null.</returns>
    Task<Game?> GetActiveGameByPlayerIdAsync(PlayerId playerId, CancellationToken cancellationToken);
}
