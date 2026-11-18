using BattleshipGame.Application.Features.Players.Queries;

namespace BattleshipGame.Application.Interfaces.Persistence;

/// <summary>
/// Read-side query for the leaderboard: player standings computed from finished games.
/// </summary>
/// <remarks>
/// The leaderboard reads across the Game and Player aggregates, so it is a query of its own
/// rather than a method of either repository.
/// </remarks>
public interface ILeaderboardReader
{
    /// <summary>
    /// Gets the top players, best first. Only players with at least one finished game appear.
    /// </summary>
    /// <remarks>
    /// Players are ordered by:
    /// <list type="number">
    /// <item>ranked (at least <paramref name="minimumFinishedGamesToRank"/> finished games)
    /// before provisional;</item>
    /// <item>win rate, highest first;</item>
    /// <item>wins against the AI opponent, most first, since beating it is harder than beating
    /// Random;</item>
    /// <item>games won, most first;</item>
    /// <item>username, so that the order is stable.</item>
    /// </list>
    /// </remarks>
    /// <param name="top">The maximum number of players to return.</param>
    /// <param name="minimumFinishedGamesToRank">
    /// The number of finished games a player needs to be ranked rather than provisional.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The players' standings, best first.</returns>
    Task<IReadOnlyList<PlayerStanding>> GetTopPlayersAsync(
        int top,
        int minimumFinishedGamesToRank,
        CancellationToken ct
    );
}
