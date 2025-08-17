using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Application.Features.Games.Queries;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Controller for managing games.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="mediator">The mediator.</param>
/// <param name="gameRepository">The game repository (for operations not yet converted to MediatR).</param>
[ApiController]
[Route("api/[controller]")]
public class GamesController(
    ILogger<GamesController> logger,
    IMediator mediator,
    IGameRepository gameRepository) : ControllerBase
{
    /// <summary>
    /// Create a new game
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GameModel), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<ActionResult<Guid>> CreateGame(
        [FromBody] CreateGameRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateGameCommand(new PlayerId(request.PlayerId), request.BoardSize);
        var result = await mediator.Send(command, ct);

        logger.LogInformation("New Game: {GameId} for Player: {PlayerId}", result.GameId, request.PlayerId);

        return CreatedAtAction(nameof(GetGame), new { id = result.GameId });
    }

    /// <summary>
    /// Get game details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<GetGameResult>> GetGame(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var query = new GetGameQuery(new GameId(id));
        var result = await mediator.Send(query, ct);
        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game Not Found",
                Detail = $"Game with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Add a ship to a board
    /// </summary>
    [HttpPost("{id:guid}/ships")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<Guid>> AddShip(
        [FromRoute] Guid id,
        [FromBody] AddShipRequest request,
        CancellationToken ct = default)
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

            var result = await mediator.Send(addShipCommand, ct);

            return Ok(result.ShipId);
        }
        catch (GameNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game Not Found",
                Detail = $"Game with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Ship Placement Error",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Ship Parameters",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Attack a cell
    /// </summary>
    [HttpPost("{id:guid}/attacks")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Attack(
        [FromRoute] Guid id,
        [FromBody] AttackRequest request,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByIdAsync(new GameId(id), cancellationToken);

        if (game is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game Not Found",
                Detail = $"Game with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        try
        {
            game.Attack(request.Side, request.Cell);
            await gameRepository.SaveAsync(game, cancellationToken);

            return Ok();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Attack",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Get game state
    /// </summary>
    [HttpGet("{id:guid}/state")]
    [ProducesResponseType(typeof(GameStateModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<GameStateModel>> GetGameState(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var game = await gameRepository.GetByIdAsync(new GameId(id), cancellationToken);

        if (game is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Game Not Found",
                Detail = $"Game with ID '{id}' was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        // For demo, winner is null unless state is GameOver
        var winner = game.State == GameState.GameOver ? game.PlayerId.Value : (Guid?)null;
        return new GameStateModel(game.State.ToString(), winner);
    }

    // DTOs and request models
    public record CreateGameRequest(Guid PlayerId, int? BoardSize = 10);

    public record AddShipRequest(
        BoardSide Side,
        ShipKind ShipKind,
        ShipOrientation Orientation,
        string BowCode
    );

    public record GameModel(Guid Id, string State, int BoardSize)
    {
        public static GameModel From(Game game) =>
            new(game.Id.Value, game.State.ToString(), game.BoardSize);
    }

    public record AttackRequest(BoardSide Side, string Cell);

    public record GameStateModel(string State, Guid? Winner);
}
