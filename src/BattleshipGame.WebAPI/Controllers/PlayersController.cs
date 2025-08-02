using BattleshipGame.Application.Features.Players.Commands.CreatePlayer;
using BattleshipGame.Application.Features.Players.Queries.GetPlayer;
using BattleshipGame.Application.Features.Players.Queries.GetPlayerByUsername;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Controller for managing player operations.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="mediator">The mediator.</param>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController(ILogger<PlayersController> logger, IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new player.
    /// </summary>
    /// <param name="request">The player creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created player information.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PlayerModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlayerModel>> CreatePlayer(
        [FromBody] CreatePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Username",
                Detail = "Username cannot be null or whitespace.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var command = new CreatePlayerCommand(request.Username);
            var result = await mediator.Send(command, cancellationToken);

           logger.LogInformation("Player created with ID: {PlayerId}, Username: {Username}",
                result.PlayerId.Value, result.Username);

            var response = new PlayerModel(
                result.PlayerId.Value,
                result.Username,
                null, // No active game initially
                0     // No games played initially
            );

            return CreatedAtAction(nameof(GetPlayer), new { id = result.PlayerId.Value }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Username Already Exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    /// <summary>
    /// Gets a player by ID.
    /// </summary>
    /// <param name="id">The player ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The player information.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlayerModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerModel>> GetPlayer(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPlayerQuery(new PlayerId(id));
        var result = await mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Player Not Found",
                Detail = $"Player with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var response = new PlayerModel(
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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The player information.</returns>
    [HttpGet("{username:alpha}")]
    [ProducesResponseType(typeof(PlayerModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerModel>> GetPlayerByUsername(
        string username,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPlayerByUsernameQuery(username);
        var result = await mediator.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Player Not Found",
                Detail = $"Player with username '{username}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var response = new PlayerModel(
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
public record PlayerModel(
    Guid Id,
    string Username,
    Guid? ActiveGameId,
    int TotalGamesPlayed
);
