using System;
using BattleshipGame.Domain.AggregateRoots;

namespace BattleshipGame.Domain.Entities;

/// <summary>
/// This represents an instance of the Battleship game, and it tracks the state of the game.
/// </summary>
internal class Game : Entity<Guid>
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

    public void AddShip(Player player, ShipKind shipKind, string bow, ShipOrientation orientation)
    {
        var board = BoardSelector(player);

        board.AddShip(shipKind, bow, orientation);
    }

    /// <summary>
    /// Attacks a cell on the specified player's board
    /// </summary>
    /// <param name="player">The player whose board to attack</param>
    /// <param name="cell">The cell to attack</param>
    /// <returns>True if the attack hit a ship, false otherwise</returns>
    public void Attack(Player player, string cell)
    {
        BoardSelector(player).Attack(cell);
    }

    /// <summary>
    /// Checks if the specified player has lost the game
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <returns>True if all player's ships have been sunk, false otherwise</returns>
    public bool IsGameOver(Player player) => BoardSelector(player).IsGameOver;

    public bool IsReady(Player player) => BoardSelector(player).IsReady;

    /// <summary>
    /// Gets the board for the specified player
    /// </summary>
    /// <param name="player">The player whose board to get</param>
    /// <returns>The player's board</returns>
    private Board BoardSelector(Player player)
    {
        return player switch
        {
            Player.Own => _ownBoard
                ?? throw new InvalidOperationException(
                    $"Board for {player} has not been created yet."
                ),
            Player.Opp => _oppBoard
                ?? throw new InvalidOperationException(
                    $"Board for {player} has not been created yet."
                ),
            _ => throw new ArgumentOutOfRangeException(nameof(player), player, null),
        };
    }
}
