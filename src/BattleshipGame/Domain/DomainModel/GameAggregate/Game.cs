using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

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

    public Game(PlayerId playerId, int boardSize = Board.DefaultSize)
    {
        PlayerId = playerId;
        BoardSize = boardSize;
        State = GameState.Started;

        _ownBoard = new Board(boardSize);
        _oppBoard = new Board(boardSize);
    }

    public ShipId AddShip(
        BoardSide boardSide,
        ShipKind shipKind,
        ShipOrientation orientation,
        string bowCode
    )
    {
        var board = BoardSelector(boardSide);
        var shipId = board.AddShip(shipKind, orientation, bowCode);
        return shipId;
    }

    /// <summary>
    /// Attacks a cell on the specified boardSide's board
    /// </summary>
    /// <param name="boardSide">The boardSide whose board to attack</param>
    /// <param name="cell">The cell to attack</param>
    /// <returns>True if the attack hit a ship, false otherwise</returns>
    public void Attack(BoardSide boardSide, string cell)
    {
        BoardSelector(boardSide).Attack(cell);
    }

    /// <summary>
    /// Checks if the specified boardSide has lost the game
    /// </summary>
    /// <param name="boardSide">The boardSide to check</param>
    /// <returns>True if all boardSide's ships have been sunk, false otherwise</returns>
    public bool IsGameOver(BoardSide boardSide) => BoardSelector(boardSide).IsGameOver;

    public bool IsReady(BoardSide boardSide) => BoardSelector(boardSide).IsReady;

    /// <summary>
    /// Gets the board for the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose board to get</param>
    /// <returns>The boardSide's board</returns>
    private Board BoardSelector(BoardSide boardSide)
    {
        return boardSide switch
        {
            BoardSide.Own => _ownBoard
                ?? throw new InvalidOperationException(
                    $"Board for {boardSide} has not been created yet."
                ),
            BoardSide.Opp => _oppBoard
                ?? throw new InvalidOperationException(
                    $"Board for {boardSide} has not been created yet."
                ),
            _ => throw new ArgumentOutOfRangeException(nameof(boardSide), boardSide, null),
        };
    }
}
