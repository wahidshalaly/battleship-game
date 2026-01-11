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
/// Provides endpoints for managing games.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="gameplayService">The gameplay application service.</param>
/// <param name="mediator">The mediator.</param>
[ApiController]
[Route("api/[controller]")]
public class GamesController(
    ILogger<GamesController> logger,
    IGameplayService gameplayService,
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
        var playerId = new PlayerId(request.PlayerId);
        var gameId = await gameplayService.StartNewGameAsync(playerId, request.BoardSize ?? 10, ct);

        logger.LogInformation(
            "New Game: {GameId} for Player: {PlayerId}",
            gameId.Value,
            request.PlayerId
        );

        return CreatedAtAction(nameof(GetGame), new { id = gameId.Value }, gameId.Value);
    }

    /// <summary>
    /// Retrieves a game.
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
        var query = new GetGameQuery(gameId);
        var game = await mediator.Send(query, ct) ?? throw new GameNotFoundException(gameId);

        return Ok(game);
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
    [ProducesResponseType(typeof(CellState), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CellState>> Attack(
        [FromRoute] Guid id,
        [FromBody] AttackRequest request,
        CancellationToken ct
    )
    {
        var gameId = new GameId(id);
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

        try
        {
            // Validate state value
            if (!request.State.Equals("started", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid State Value",
                        Detail =
                            $"Invalid state value '{request.State}'. Allowed values: 'started'.",
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

            return Ok(new GameStateResponse(game.State, null));
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
        var query = new GetGameQuery(gameId);
        var game = await mediator.Send(query, ct) ?? throw new GameNotFoundException(gameId);

        // For demo, winner is null unless state is GameOver
        var winner = game.State == nameof(GameState.GameOver) ? game.PlayerId : (Guid?)null;
        return new GameStateResponse(game.State, winner);
    }
}

public record CreateGameRequest(Guid PlayerId, int? BoardSize = 10);

public record PlaceShipRequest(
    BoardSide Side,
    ShipKind ShipKind,
    ShipOrientation Orientation,
    string BowCode
);

public record AttackRequest(string Cell);

public record UpdateGameStateRequest(string State);

public record GameStateResponse(string State, Guid? Winner);
