using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.Extensions.Options;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// Query to get the top players on the leaderboard.
/// </summary>
/// <param name="Top">The maximum number of players to return.</param>
public record GetLeaderboardQuery(int Top) : IRequest<GetLeaderboardQueryResult>;

/// <summary>
/// Result of getting the leaderboard.
/// </summary>
/// <param name="MinimumFinishedGamesToRank">
/// The number of finished games a player needs to be ranked rather than provisional.
/// </param>
/// <param name="Entries">The players, best first.</param>
public record GetLeaderboardQueryResult(
    int MinimumFinishedGamesToRank,
    IReadOnlyList<LeaderboardEntry> Entries
);

/// <summary>
/// A player's line on the leaderboard.
/// </summary>
/// <param name="Rank">The player's position, starting at 1.</param>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="GamesFinished">The number of finished games.</param>
/// <param name="GamesWon">The number of finished games won.</param>
/// <param name="AiGamesWon">The number of games won against the AI opponent.</param>
/// <param name="WinRate">The share of finished games won, from 0 to 1.</param>
/// <param name="IsProvisional">
/// Whether the player has too few finished games to be ranked by win rate yet.
/// </param>
public record LeaderboardEntry(
    int Rank,
    PlayerId PlayerId,
    string Username,
    int GamesFinished,
    int GamesWon,
    int AiGamesWon,
    double WinRate,
    bool IsProvisional
);

/// <summary>
/// Handler for getting the leaderboard.
/// </summary>
/// <param name="leaderboardReader">The leaderboard reader.</param>
/// <param name="options">The leaderboard options.</param>
public class GetLeaderboardHandler(
    ILeaderboardReader leaderboardReader,
    IOptions<LeaderboardOptions> options
) : IRequestHandler<GetLeaderboardQuery, GetLeaderboardQueryResult>
{
    private readonly int _minimumFinishedGamesToRank = options.Value.MinimumFinishedGamesToRank;

    /// <inheritdoc />
    public async Task<GetLeaderboardQueryResult> Handle(
        GetLeaderboardQuery request,
        CancellationToken ct
    )
    {
        var standings = await leaderboardReader.GetTopPlayersAsync(
            request.Top,
            _minimumFinishedGamesToRank,
            ct
        );

        return new GetLeaderboardQueryResult(
            _minimumFinishedGamesToRank,
            [.. standings.Select(ToEntry)]
        );
    }

    private LeaderboardEntry ToEntry(PlayerStanding standing, int index) =>
        new(
            Rank: index + 1,
            standing.PlayerId,
            standing.Username,
            standing.Results.GamesFinished,
            standing.Results.GamesWon,
            standing.AiGamesWon,
            standing.Results.WinRate,
            IsProvisional: standing.Results.GamesFinished < _minimumFinishedGamesToRank
        );
}
