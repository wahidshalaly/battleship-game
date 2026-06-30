using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Provides endpoints for managing games from Player perspective.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="gameplayService">The gameplay application service.</param>
/// <param name="playerService">Resolves the authenticated caller's player profile.</param>
/// <param name="gameAccessGuard">Enforces that callers only access games they own.</param>
/// <param name="mediator">The mediator.</param>
[ApiController]
[Route("api/[controller]")]
public class GamesController(
    ILogger<GamesController> logger,
    IGameplayService gameplayService,
    IPlayerService playerService,
    IGameAccessGuard gameAccessGuard,
    IMediator mediator
) : ControllerBase
{
    /// <summary>
    /// Creates a new game.
    /// </summary>
    /// <response code="201">Game successfully created.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartNewGame(
        [FromBody] CreateGameRequest request,
        CancellationToken ct
    )
    {
        // The owner is the authenticated caller, not a client-supplied id.
        var player = await playerService.GetCurrentRequiredAsync(ct);
        var gameId = await gameplayService.StartNewGameAsync(
            player.Id,
            request.BoardSize ?? 10,
            request.OpponentStrategy ?? OpponentStrategy.Random,
            ct
        );

        logger.LogInformation(
            "New Game: {GameId} for Player: {PlayerId}",
            gameId.Value,
            player.Id.Value
        );

        return CreatedAtAction(nameof(GetGame), new { id = gameId.Value }, gameId.Value);
    }

    /// <summary>
    /// Retrieves a game by ID.
    /// </summary>
    /// <response code="200">Returns a game.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="404">Game not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetGameQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetGameQueryResult>> GetGame(
        [FromRoute] Guid id,
        CancellationToken ct
    )
    {
        var gameId = new GameId(id);
        await gameAccessGuard.EnsureOwnerAsync(gameId, ct);
        var query = new GetGameQuery(gameId);
        var game = await mediator.Send(query, ct) ?? throw new GameNotFoundException(gameId);

        return Ok(game);
    }

    /// <summary>
    /// Returns the caller's active game, or 204 No Content when none is in progress.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(GetGameQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetGameQueryResult>> GetActiveGame(CancellationToken ct)
    {
        var player = await playerService.GetCurrentRequiredAsync(ct);
        if (player.ActiveGameId is null)
            return NoContent();

        var query = new GetGameQuery(player.ActiveGameId);
        var game = await mediator.Send(query, ct);
        return game is null ? NoContent() : Ok(game);
    }

    /// <summary>
    /// Adds a ship to a certain board side.
    /// </summary>
    [HttpPost("{id:guid}/ships")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PlaceShip(
        [FromRoute] Guid id,
        [FromBody] PlaceShipRequest request,
        CancellationToken ct
    )
    {
        var gameId = new GameId(id);
        await gameAccessGuard.EnsureOwnerAsync(gameId, ct);
        var shipId = await gameplayService.PlaceShipAsync(
            gameId,
            request.Side,
            request.ShipKind,
            request.Orientation,
            request.BowCode,
            ct
        );

        return Ok(shipId.Value);
    }

    /// <summary>
    /// Attacks a cell on a certain board side.
    /// </summary>
    [HttpPost("{id:guid}/attacks")]
    [ProducesResponseType(typeof(LastRoundResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LastRoundResult>> Attack(
        [FromRoute] Guid id,
        [FromBody] AttackRequest request,
        CancellationToken ct
    )
    {
        var gameId = new GameId(id);
        await gameAccessGuard.EnsureOwnerAsync(gameId, ct);
        var result = await gameplayService.PlayerAttackThenCounterAttackAsync(
            gameId,
            request.Cell,
            ct
        );

        return Ok(result);
    }

    /// <summary>
    /// Updates game state (e.g., transitions from Ready to Started).
    /// </summary>
    /// <remarks>
    /// This endpoint manages game state transitions. Currently supports transitioning from Ready to Started state.
    ///
    /// Usage:
    /// PUT /api/games/{id}/state
    /// { "state": "started" }
    ///
    /// Valid transitions:
    /// - "started": Transitions game from Ready state to Started state (allows attacks to begin)
    /// </remarks>
    /// <response code="200">Game state successfully updated.</response>
    /// <response code="400">Invalid state transition or invalid state value.</response>
    /// <response code="404">Game not found.</response>
    /// <response code="409">Invalid current state for requested transition.</response>
    [HttpPut("{id:guid}/state")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameStateResponse>> UpdateGameState(
        [FromRoute] Guid id,
        [FromBody] UpdateGameStateRequest request,
        CancellationToken ct
    )
    {
        var gameId = new GameId(id);
        await gameAccessGuard.EnsureOwnerAsync(gameId, ct);

        try
        {
            // Validate state value
            if (request.State != GameState.Started)
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid State Value",
                        Detail =
                            $"Invalid state value '{request.State}'. Only 'Started' state transition is allowed.",
                        Status = StatusCodes.Status400BadRequest,
                    }
                );
            }

            // Execute StartGameplay command
            var command = new StartGameplayCommand(gameId);
            await mediator.Send(command, ct);

            logger.LogInformation("Game {GameId} transitioned to Started state", gameId.Value);

            // Retrieve updated game state
            var query = new GetGameQuery(gameId);
            var game = await mediator.Send(query, ct) ?? throw new GameNotFoundException(id);

            return Ok(new GameStateResponse(game.State, game.WinnerSide));
        }
        catch (GameNotReadyException ex)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Invalid State Transition",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                }
            );
        }
    }

    /// <summary>
    /// Retrieves a game state.
    /// </summary>
    [HttpGet("{id:guid}/state")]
    [ProducesResponseType(typeof(GameStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameStateResponse>> GetGameState(
        [FromRoute] Guid id,
        CancellationToken ct
    )
    {
        var gameId = new GameId(id);
        await gameAccessGuard.EnsureOwnerAsync(gameId, ct);
        var query = new GetGameQuery(gameId);
        var game = await mediator.Send(query, ct) ?? throw new GameNotFoundException(gameId);

        return new GameStateResponse(game.State, game.WinnerSide);
    }
}

public record CreateGameRequest(int? BoardSize = 10, OpponentStrategy? OpponentStrategy = null);

public record PlaceShipRequest(
    BoardSide Side,
    ShipKind ShipKind,
    ShipOrientation Orientation,
    string BowCode
);

public record AttackRequest(string Cell);

public record UpdateGameStateRequest(GameState State);

public record GameStateResponse(GameState State, BoardSide Winner);
