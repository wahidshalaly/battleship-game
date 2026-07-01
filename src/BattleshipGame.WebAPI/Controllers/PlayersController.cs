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
        return Ok(ToResponse(player));
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
        return Ok(
            new PlayerResponse(
                result.PlayerId.Value,
                result.Username,
                result.ActiveGameId,
                result.TotalGamesPlayed
            )
        );
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
        return Ok(
            new PlayerResponse(
                result.PlayerId.Value,
                result.Username,
                result.ActiveGameId,
                result.TotalGamesPlayed
            )
        );
    }

    private static PlayerResponse ToResponse(Player player) =>
        new(player.Id.Value, player.Username, player.ActiveGameId?.Value, player.TotalGamesPlayed);
}

/// <param name="Id">The player's unique identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="ActiveGameId">The currently active game ID, if any.</param>
/// <param name="TotalGamesPlayed">The total number of games played.</param>
public record PlayerResponse(Guid Id, string Username, Guid? ActiveGameId, int TotalGamesPlayed);
