using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.Infrastructure.Persistence.Queries;

internal class LeaderboardReader(BattleshipGameDbContext context) : ILeaderboardReader
{
    public async Task<IReadOnlyList<PlayerStanding>> GetTopPlayersAsync(
        int top,
        int minimumFinishedGamesToRank,
        CancellationToken ct
    )
    {
        var resultsByPlayer = context
            .Games.Where(g =>
                g.State == (int)GameState.GameOver && g.WinnerSide != (int)BoardSide.None
            )
            .GroupBy(g => g.PlayerId)
            .Select(games => new
            {
                PlayerId = games.Key,
                GamesFinished = games.Count(),
                GamesWon = games.Count(g => g.WinnerSide == (int)BoardSide.Player),
                AiGamesWon = games.Count(g =>
                    g.WinnerSide == (int)BoardSide.Player
                    && g.OpponentStrategy == (int)OpponentStrategy.SemanticKernel
                ),
            });

        // Sorted and cut in the database, so only the top rows are read.
        var rows = await resultsByPlayer
            .Join(
                context.Players,
                results => results.PlayerId,
                player => player.Id,
                (results, player) =>
                    new
                    {
                        results.PlayerId,
                        player.Username,
                        results.GamesFinished,
                        results.GamesWon,
                        results.AiGamesWon,
                    }
            )
            .OrderByDescending(r => r.GamesFinished >= minimumFinishedGamesToRank)
            .ThenByDescending(r => (double)r.GamesWon / r.GamesFinished)
            .ThenByDescending(r => r.AiGamesWon)
            .ThenByDescending(r => r.GamesWon)
            .ThenBy(r => r.Username)
            .Take(top)
            .ToListAsync(ct);

        return
        [
            .. rows.Select(r => new PlayerStanding(
                new PlayerId(r.PlayerId),
                r.Username,
                new GameResults(r.GamesWon, r.GamesFinished - r.GamesWon),
                r.AiGamesWon
            )),
        ];
    }
}
