using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// A player's standing on the leaderboard, read from their finished games.
/// </summary>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="Results">The outcomes of the player's finished games.</param>
/// <param name="AiGamesWon">
/// The number of games won against the AI opponent (<c>SemanticKernel</c>), included in
/// <see cref="GameResults.GamesWon"/>.
/// </param>
public record PlayerStanding(
    PlayerId PlayerId,
    string Username,
    GameResults Results,
    int AiGamesWon
);
