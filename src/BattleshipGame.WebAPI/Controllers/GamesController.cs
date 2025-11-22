using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Provides endpoints for managing games.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="mediator">The mediator.</param>
/// <param name="gameRepository">The game repository (for operations not yet converted to MediatR).</param>
[ApiController]
[Route("api/[controller]")]
public class GamesController(ILogger<GamesController> logger, IMediator mediator, IGameRepository gameRepository)
    : ControllerBase
{
    /// <summary>
    /// Creates a new game.
    /// </summary>
    /// <response code="201">Game successfully created.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGame(
        [FromBody] CreateGameRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(
            new CreateGameCommand(new PlayerId(request.PlayerId), request.BoardSize),
            cancellationToken
        );

        logger.LogInformation("New Game: {GameId} for Player: {PlayerId}", result.GameId, request.PlayerId);

        return CreatedAtAction(nameof(GetGame), new { id = result.GameId.Value }, result.GameId);
    }

    /// <summary>
    /// Retrieves a game.
    /// </summary>
    /// <response code="200">Returns a game.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="404">Game not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameModel>> GetGame([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGameQuery(new GameId(id));
        var result = await mediator.Send<GameModel?>(query, cancellationToken);
        if (result is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Game Not Found",
                    Detail = $"Game with ID '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Adds a ship to a certain board side.
    /// </summary>
    [HttpPost("{id:guid}/ships")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> AddShip(
        [FromRoute] Guid id,
        [FromBody] AddShipRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var addShipCommand = new AddShipCommand(
                new GameId(id),
                request.Side,
                request.ShipKind,
                request.Orientation,
                request.BowCode
            );

            var result = await mediator.Send(addShipCommand, cancellationToken);

            return Ok(result.ShipId);
        }
        catch (GameNotFoundException)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Game Not Found",
                    Detail = $"Game with ID '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Ship Placement Error",
                    Detail = exception.Message,
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Invalid Ship Parameters",
                    Detail = exception.Message,
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }
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
        CancellationToken cancellationToken
    )
    {
        var game = await gameRepository.GetByIdAsync(new GameId(id), cancellationToken);

        if (game is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Game Not Found",
                    Detail = $"Game with ID '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        try
        {
            var cellState = game.Attack(request.Side, request.Cell);
            await gameRepository.SaveAsync(game, cancellationToken);

            return Ok(cellState);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Title = "Invalid Attack",
                    Detail = exception.Message,
                    Status = StatusCodes.Status400BadRequest,
                }
            );
        }
    }

    /// <summary>
    /// Retrieves a game state.
    /// </summary>
    [HttpGet("{id:guid}/state")]
    [ProducesResponseType(typeof(GameStateModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameStateModel>> GetGameState(
        [FromRoute] Guid id,
        CancellationToken cancellationToken
    )
    {
        var game = await gameRepository.GetByIdAsync(new GameId(id), cancellationToken);

        if (game is null)
        {
            return NotFound(
                new ProblemDetails
                {
                    Title = "Game Not Found",
                    Detail = $"Game with ID '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );
        }

        // For demo, winner is null unless state is GameOver
        var winner = game.State == GameState.GameOver ? game.PlayerId.Value : (Guid?)null;
        return new GameStateModel(game.State.ToString(), winner);
    }
}

public record CreateGameRequest(Guid PlayerId, int? BoardSize = 10);

public record AddShipRequest(BoardSide Side, ShipKind ShipKind, ShipOrientation Orientation, string BowCode);

public record AttackRequest(BoardSide Side, string Cell);

public record GameStateModel(string State, Guid? Winner);
