using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Provides endpoints for managing players.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="playerService">The player application service.</param>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController(ILogger<PlayersController> logger, IPlayerService playerService)
    : ControllerBase
{
    /// <summary>
    /// Creates a new player.
    /// </summary>
    /// <param name="request">The player creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created player information.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> CreatePlayer(
        [FromBody] CreatePlayerRequest request,
        CancellationToken ct
    )
    {
        // Delegate to service for creation + validation
        var playerId = await playerService.CreateAsync(request.Username, ct);

        // TODO: consider removing sensitive info from logs
        logger.LogInformation(
            "Player created with ID: {PlayerId}, Username: {Username}",
            playerId.Value,
            request.Username
        );

        return CreatedAtAction(nameof(GetPlayerById), new { id = playerId.Value }, playerId.Value);
    }

    /// <summary>
    /// Gets a player by ID.
    /// </summary>
    /// <param name="id">The player ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The player information.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> GetPlayerById(Guid id, CancellationToken ct)
    {
        var result =
            await playerService.GetByIdAsync(new PlayerId(id), ct)
            ?? throw new PlayerNotFoundException(id);
        var response = new PlayerResponse(
            result.PlayerId.Value,
            result.Username,
            result.ActiveGameId,
            result.TotalGamesPlayed
        );

        return Ok(response);
    }

    /// <summary>
    /// Gets a player by username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The player information.</returns>
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
        var response = new PlayerResponse(
            result.PlayerId.Value,
            result.Username,
            result.ActiveGameId,
            result.TotalGamesPlayed
        );

        return Ok(response);
    }
}

/// <summary>
/// Request model for creating a new player.
/// </summary>
/// <param name="Username">The player's username.</param>
public record CreatePlayerRequest(string Username);

/// <summary>
/// Response model for player information.
/// </summary>
/// <param name="Id">The player's unique identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="ActiveGameId">The currently active game ID, if any.</param>
/// <param name="TotalGamesPlayed">The total number of games played.</param>
public record PlayerResponse(Guid Id, string Username, Guid? ActiveGameId, int TotalGamesPlayed);
