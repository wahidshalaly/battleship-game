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
);
