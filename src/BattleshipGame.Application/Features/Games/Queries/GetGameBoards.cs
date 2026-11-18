using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Queries;

/// <summary>
/// Query to get the player's view of both boards of a game.
/// </summary>
/// <param name="GameId">The game identifier.</param>
public record GetGameBoardsQuery(GameId GameId) : IRequest<GetGameBoardsQueryResult?>;

/// <summary>
/// The player's view of both boards of a game: everything on their own board, and only what
/// their attacks have revealed on the opponent's board.
/// </summary>
/// <param name="GameId">The game's identifier.</param>
/// <param name="OwnBoard">The player's board: every ship, and the opponent's hits and misses.</param>
/// <param name="OpponentBoard">
/// The opponent's board: the player's hits and misses, and only the ships the player has sunk.
/// </param>
public record GetGameBoardsQueryResult(Guid GameId, BoardView OwnBoard, BoardView OpponentBoard)
{
    /// <summary>
    /// Creates the player's view of the boards of a game.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <returns>The boards result.</returns>
    internal static GetGameBoardsQueryResult From(Game game) =>
        new(
            game.Id.Value,
            BoardView.From(game, BoardSide.Player, revealUnsunkShips: true),
            BoardView.From(game, BoardSide.Opponent, revealUnsunkShips: false)
        );
}

/// <summary>
/// What a player can see of one board.
/// </summary>
/// <param name="Ships">The visible ships on the board.</param>
/// <param name="Hits">The attacked cells that hit a ship.</param>
/// <param name="Misses">The attacked cells that missed.</param>
public record BoardView(
    IReadOnlyList<ShipView> Ships,
    IReadOnlyList<string> Hits,
    IReadOnlyList<string> Misses
)
{
    /// <summary>
    /// Creates the view of one board of a game.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="side">The board side to view.</param>
    /// <param name="revealUnsunkShips">Whether ships that are still afloat are visible.</param>
    /// <returns>The board view.</returns>
    internal static BoardView From(Game game, BoardSide side, bool revealUnsunkShips)
    {
        var ships = game.GetShips(side)
            .Where(id => revealUnsunkShips || game.IsShipSunk(side, id))
            .Select(id => ShipView.From(game, side, id))
            .ToList();

        return new BoardView(ships, [.. game.GetHits(side)], [.. game.GetMisseds(side)]);
    }
}

/// <summary>
/// A ship as shown on a board.
/// </summary>
/// <param name="Kind">The kind of ship.</param>
/// <param name="Position">The cells the ship occupies.</param>
/// <param name="Sunk">Whether every cell of the ship has been hit.</param>
public record ShipView(ShipKind Kind, IReadOnlyList<string> Position, bool Sunk)
{
    /// <summary>
    /// Creates the view of a ship on a board of a game.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="side">The board side the ship is on.</param>
    /// <param name="shipId">The ship's identifier.</param>
    /// <returns>The ship view.</returns>
    internal static ShipView From(Game game, BoardSide side, ShipId shipId) =>
        new(
            game.GetShipKind(side, shipId),
            [.. game.GetShipPosition(side, shipId)],
            game.IsShipSunk(side, shipId)
        );
}

/// <summary>
/// Handler for getting the player's view of both boards of a game.
/// </summary>
/// <param name="gameRepository">The game repository to access game data.</param>
internal class GetGameBoardsHandler(IGameRepository gameRepository)
    : IRequestHandler<GetGameBoardsQuery, GetGameBoardsQueryResult?>
{
    /// <inheritdoc />
    public async Task<GetGameBoardsQueryResult?> Handle(
        GetGameBoardsQuery request,
        CancellationToken ct
    )
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, ct);

        return game is null ? null : GetGameBoardsQueryResult.From(game);
    }
}
