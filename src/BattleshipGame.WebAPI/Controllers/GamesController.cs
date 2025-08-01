using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    // In-memory store for demo (replace with real persistence in production)
    private static readonly Dictionary<Guid, Game> _games = new();

    /// <summary>
    /// Create a new game
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GameModel), 201)]
    public ActionResult<GameModel> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = new Game(request.PlayerId, request.BoardSize ?? 10);
        _games[game.Id] = game;
        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, GameModel.From(game));
    }

    /// <summary>
    /// Get game details
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameModel), 200)]
    [ProducesResponseType(404)]
    public ActionResult<GameModel> GetGame([FromRoute] Guid id)
    {
        if (!_games.TryGetValue(id, out var game))
            return NotFound();
        return GameModel.From(game);
    }

    /// <summary>
    /// Add a ship to a board
    /// </summary>
    [HttpPost("{id:guid}/ships")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public ActionResult<Guid> AddShip([FromRoute] Guid id, [FromBody] AddShipRequest request)
    {
        if (!_games.TryGetValue(id, out var game))
            return NotFound();
        try
        {
            var shipId = game.AddShip(
                request.Side,
                request.ShipKind,
                request.Orientation,
                request.Bow
            );
            return Ok(shipId);
        }
        catch (ApplicationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    /// <summary>
    /// Attack a cell
    /// </summary>
    [HttpPost("{id:guid}/attack")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult Attack([FromRoute] Guid id, [FromBody] AttackRequest request)
    {
        if (!_games.TryGetValue(id, out var game))
            return NotFound();

        try
        {
            // For demo, assume a hit if cell ends with '1', sunk if cell ends with '5', game over if cell ends with '9'
            // Replace with real logic using game.Attack(request.Side, request.Cell)
            game.Attack(request.Side, request.Cell);
            return Ok();
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }

    /// <summary>
    /// Get game state
    /// </summary>
    [HttpGet("{id:guid}/state")]
    [ProducesResponseType(typeof(GameStateModel), 200)]
    [ProducesResponseType(404)]
    public ActionResult<GameStateModel> GetGameState([FromRoute] Guid id)
    {
        if (!_games.TryGetValue(id, out var game))
            return NotFound();

        // For demo, winner is null unless state is GameOver
        var winner = game.State == GameState.GameOver ? game.Id : null;
        return new GameStateModel(game.State.ToString(), winner!);
    }

    // DTOs and request models
    public record CreateGameRequest(PlayerId PlayerId, int? BoardSize = 10);

    public record AddShipRequest(
        BoardSide Side,
        ShipKind ShipKind,
        ShipOrientation Orientation,
        string Bow
    );

    public record GameModel(Guid Id, string State, int BoardSize)
    {
        public static GameModel From(Game game) =>
            new(game.Id, game.State.ToString(), game.BoardSize);
    }

    public record AttackRequest(BoardSide Side, string Cell);

    public record GameStateModel(string State, Guid? Winner);
}
