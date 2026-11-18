using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Provides endpoints for reading player profiles.
/// Player creation is handled by POST /api/auth/register.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController(ILogger<PlayersController> logger, IPlayerService playerService)
    : ControllerBase
{
    /// <summary>
    /// Returns the authenticated caller's own player profile.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlayerResponse>> GetMe(CancellationToken ct)
    {
        var player = await playerService.GetCurrentRequiredAsync(ct);
        logger.LogDebug("GET /me resolved player {PlayerId}", player.Id.Value);
        var result =
            await playerService.GetByIdAsync(player.Id, ct)
            ?? throw new PlayerNotFoundException(player.Id);
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// Gets a player by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> GetPlayerById(Guid id, CancellationToken ct)
    {
        var result =
            await playerService.GetByIdAsync(new PlayerId(id), ct)
            ?? throw new PlayerNotFoundException(id);
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// Gets a player by username.
    /// </summary>
    [HttpGet("{username:alpha}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> GetPlayerByUsername(
        string username,
        CancellationToken ct
    )
    {
        var result =
            await playerService.GetByUsernameAsync(username, ct)
            ?? throw new PlayerNotFoundException(username);
        return Ok(ToResponse(result));
    }

    /// <summary>
    /// Gets the top players on the leaderboard, best first. Players are ranked by win rate once
    /// they have finished enough games; until then they are provisional and listed after the
    /// ranked players.
    /// </summary>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(LeaderboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeaderboardResponse>> GetLeaderboard(
        [FromQuery] LeaderboardRequest request,
        CancellationToken ct
    )
    {
        var result = await playerService.GetLeaderboardAsync(request.Top, ct);
        return Ok(ToResponse(result));
    }

    private static PlayerResponse ToResponse(GetPlayerQueryResult result) =>
        new(
            result.PlayerId.Value,
            result.Username,
            result.ActiveGameId,
            result.TotalGamesPlayed,
            result.GamesWon,
            result.GamesLost,
            result.WinRate
        );

    private static LeaderboardResponse ToResponse(GetLeaderboardQueryResult result) =>
        new(result.MinimumFinishedGamesToRank, [.. result.Entries.Select(ToResponse)]);

    private static LeaderboardEntryResponse ToResponse(LeaderboardEntry entry) =>
        new(
            entry.Rank,
            entry.PlayerId.Value,
            entry.Username,
            entry.GamesFinished,
            entry.GamesWon,
            entry.AiGamesWon,
            entry.WinRate,
            entry.IsProvisional
        );
}

/// <param name="Id">The player's unique identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="ActiveGameId">The currently active game ID, if any.</param>
/// <param name="TotalGamesPlayed">The total number of games played, finished or not.</param>
/// <param name="GamesWon">The number of finished games the player won.</param>
/// <param name="GamesLost">The number of finished games the player lost.</param>
/// <param name="WinRate">The share of finished games won, from 0 to 1.</param>
public record PlayerResponse(
    Guid Id,
    string Username,
    Guid? ActiveGameId,
    int TotalGamesPlayed,
    int GamesWon,
    int GamesLost,
    double WinRate
);

/// <summary>
/// Query parameters of the leaderboard request.
/// </summary>
public sealed class LeaderboardRequest
{
    /// <summary>
    /// The maximum number of players to return, from 1 to 100.
    /// </summary>
    [FromQuery(Name = "top")]
    public int Top { get; init; } = 10;
}

/// <param name="MinimumFinishedGamesToRank">
/// The number of finished games a player needs to be ranked rather than provisional.
/// </param>
/// <param name="Entries">The players, best first.</param>
public record LeaderboardResponse(
    int MinimumFinishedGamesToRank,
    IReadOnlyList<LeaderboardEntryResponse> Entries
);

/// <param name="Rank">The player's position, starting at 1.</param>
/// <param name="PlayerId">The player's unique identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="GamesFinished">The number of finished games.</param>
/// <param name="GamesWon">The number of finished games won.</param>
/// <param name="AiGamesWon">The number of games won against the AI opponent.</param>
/// <param name="WinRate">The share of finished games won, from 0 to 1.</param>
/// <param name="IsProvisional">
/// Whether the player has too few finished games to be ranked by win rate yet.
/// </param>
public record LeaderboardEntryResponse(
    int Rank,
    Guid PlayerId,
    string Username,
    int GamesFinished,
    int GamesWon,
    int AiGamesWon,
    double WinRate,
    bool IsProvisional
);
