using System;
using BattleshipGame.Domain.Common;

namespace BattleshipGame.Domain.GameAggregate;

/// <summary>
/// This represents an instance of the Battleship game, and it tracks the state of the game.
/// </summary>
public sealed class Game : AggregateRoot<Guid>
{
    private readonly Board _ownBoard;
    private readonly Board _oppBoard;

    public GameState State { get; private set; }

    public Game(int boardSize = Board.DefaultSize)
    {
        _ownBoard = new Board(boardSize);
        _oppBoard = new Board(boardSize);

        Id = Guid.NewGuid();
        State = GameState.Started;
    }

    public void AddShip(
        BoardSide boardSide,
        ShipKind shipKind,
        string bow,
        ShipOrientation orientation
    )
    {
        var board = BoardSelector(boardSide);

        board.AddShip(shipKind, bow, orientation);
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
