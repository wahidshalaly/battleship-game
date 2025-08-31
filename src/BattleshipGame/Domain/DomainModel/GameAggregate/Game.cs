using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Represents the unique identifier for a game.
/// </summary>
/// <remarks>This type encapsulates a <see cref="Guid"/> value to uniquely identify a game entity. It inherits
/// from <see cref="EntityId"/> to provide additional context or functionality specific to entity
/// identification.</remarks>
/// <param name="Value"></param>
public record GameId(Guid Value) : EntityId(Value);

/// <summary>
/// This represents an instance of the Battleship game, and it tracks the state of the game.
/// </summary>
public sealed class Game : AggregateRoot<GameId>
{
    private readonly Board _ownBoard;
    private readonly Board _oppBoard;

    public PlayerId PlayerId { get; init; }

    public int BoardSize { get; init; }

    public GameState State { get; private set; }

    public Game(PlayerId playerId, int boardSize = DefaultBoardSize)
    {
        PlayerId = playerId;
        BoardSize = boardSize;
        State = GameState.Started;

        _ownBoard = new Board(boardSize);
        _oppBoard = new Board(boardSize);
    }

    public ShipId AddShip(BoardSide side, ShipKind kind, ShipOrientation orientation, string bowCode)
    {
        var board = BoardSelector(side);
        var shipId = board.AddShip(kind, orientation, bowCode);

        if (BoardSelector(side).IsReady)
        {
            AddDomainEvent(new BoardSideReadyEvent(Id, side));
        }

        // Check if both boards are now ready and raise event
        if (IsReady)
        {
            State = GameState.BoardsAreReady;
            AddDomainEvent(new BoardsReadyEvent(Id));
        }

        return shipId;
    }

    /// <summary>
    /// Attacks a cell on the specified boardSide's board
    /// </summary>
    /// <param name="boardSide">The boardSide whose board to attack</param>
    /// <param name="cellCode">The cell to attack</param>
    /// <returns>True if the attack hit a ship, false otherwise</returns>
    public CellState Attack(BoardSide boardSide, string cellCode)
    {
        var board = BoardSelector(boardSide);
        var (cellState, shipId, shipSunk) = board.Attack(cellCode);

        // Raise domain event for cell attack
        AddDomainEvent(new CellAttackedEvent(Id, cellCode, cellState));
        if (cellState != CellState.Hit)
            return cellState;

        // Check if the cell belongs to a ship that was sunk
        if (shipId is not null && shipSunk)
        {
            // Raise domain event if a ship was sunk
            AddDomainEvent(new ShipSunkEvent(Id, shipId, boardSide));
        }

        // Check if the game is over for the attacked boardSide
        if (IsGameOver(boardSide))
        {
            // Raise domain event if a game is over
            State = GameState.GameOver;
            AddDomainEvent(new GameOverEvent(Id, boardSide));
        }

        return cellState;
    }

    /// <summary>
    /// Checks if the specified boardSide has lost the game
    /// </summary>
    /// <param name="boardSide">The boardSide to check</param>
    /// <returns>True if all boardSide's ships have been sunk, false otherwise</returns>
    public bool IsGameOver(BoardSide boardSide) => BoardSelector(boardSide).IsGameOver;

    public bool IsBoardReady(BoardSide boardSide) => BoardSelector(boardSide).IsReady;

    public bool IsReady => IsBoardReady(BoardSide.Own) && IsBoardReady(BoardSide.Opp);

    public IReadOnlyCollection<ShipId> GetShips(BoardSide boardSide)
    {
        return BoardSelector(boardSide).Ships.Select(s => s.Id).ToList().AsReadOnly();
    }

    public IReadOnlyCollection<string> GetShipPosition(BoardSide boardSide, ShipId shipId)
    {
        return BoardSelector(boardSide).Ships.First(s => s.Id == shipId).Position;
    }

    /// <summary>
    /// Gets the board for the specified side
    /// </summary>
    /// <param name="side">The side whose board to get</param>
    /// <returns>The side's board</returns>
    private Board BoardSelector(BoardSide side)
    {
        return side switch
        {
            BoardSide.Own => _ownBoard,
            BoardSide.Opp => _oppBoard,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, ErrorMessages.InvalidBoardSide),
        };
    }
}
