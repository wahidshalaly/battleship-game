using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
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
        var gameId = await gameplayService.StartNewGameAsync(
            new PlayerId(request.PlayerId),
            request.BoardSize ?? 10,
            ct
        );

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
        var query = new GetGameQuery(new GameId(id));
        var game =
            await mediator.Send(query, ct) ?? throw new GameNotFoundException(new GameId(id));

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
        var shipId = await gameplayService.PlaceShipAsync(
            new GameId(id),
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
        var result = await gameplayService.PlayerAttackAndCounterAttackAsync(
            new GameId(id),
            request.Cell,
            ct
        );

        return Ok(result);
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
        var query = new GetGameQuery(new GameId(id));
        var game =
            await mediator.Send(query, ct) ?? throw new GameNotFoundException(new GameId(id));

        // For demo, winner is null unless state is GameOver
        var winner = game.State == GameState.GameOver ? game.PlayerId : (Guid?)null;
        return new GameStateResponse(game.State.ToString(), winner);
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

public record GameStateResponse(string State, Guid? Winner);
