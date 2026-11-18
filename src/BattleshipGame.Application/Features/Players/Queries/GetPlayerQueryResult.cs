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
    /// <summary>
    /// Creates the result for a player, reading their active game and game count from games.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="gameRepository">The game repository.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The player result.</returns>
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
