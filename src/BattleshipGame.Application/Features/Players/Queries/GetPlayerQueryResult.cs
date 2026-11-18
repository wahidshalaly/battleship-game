using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// Result of getting a player.
/// </summary>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="ActiveGameId">The currently active game ID, if any.</param>
/// <param name="TotalGamesPlayed">The total number of games played.</param>
public record GetPlayerQueryResult(
    PlayerId PlayerId,
    string Username,
    Guid? ActiveGameId,
    int TotalGamesPlayed
)
{
    internal static async Task<GetPlayerQueryResult> FromAsync(
        Player player,
        IGameRepository gameRepository,
        CancellationToken ct
    )
    {
        var activeGame = await gameRepository.GetActiveGameByPlayerIdAsync(player.Id, ct);
        var totalGamesPlayed = await gameRepository.CountByPlayerIdAsync(player.Id, ct);
        return new GetPlayerQueryResult(
            player.Id,
            player.Username,
            activeGame?.Id.Value,
            totalGamesPlayed
        );
    }
}
