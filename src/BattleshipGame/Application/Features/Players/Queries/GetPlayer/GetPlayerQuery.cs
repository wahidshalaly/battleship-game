using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries.GetPlayer;

/// <summary>
/// Query to get a player by ID.
/// </summary>
/// <param name="PlayerId">The player identifier.</param>
public record GetPlayerQuery(PlayerId PlayerId) : IRequest<GetPlayerResult>;

/// <summary>
/// Result of getting a player.
/// </summary>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="ActiveGameId">The currently active game ID, if any.</param>
/// <param name="TotalGamesPlayed">The total number of games played.</param>
public record GetPlayerResult(
    PlayerId PlayerId,
    string Username,
    Guid? ActiveGameId,
    int TotalGamesPlayed
);
