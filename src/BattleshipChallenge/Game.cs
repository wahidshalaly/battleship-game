using System;

namespace BattleshipChallenge;

/// <summary>
/// This represents an instance of Ship game, and it tracks the state of the game.
/// </summary>
public class Game
{
    private readonly Board _player1Board;
    private readonly Board _player2Board;

    /// <summary>
    /// Unique identifier for this game instance
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    public GameState State { get; private set; } = GameState.Started;

    public Game(int boardSize = Board.DefaultSize)
    {
        _player1Board = new Board(boardSize);
        _player2Board = new Board(boardSize);
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
            Player.One => _player1Board
                ?? throw new InvalidOperationException(
                    $"Board for {player} has not been created yet."
                ),
            Player.Two => _player2Board
                ?? throw new InvalidOperationException(
                    $"Board for {player} has not been created yet."
                ),
            _ => throw new ArgumentOutOfRangeException(nameof(player), player, null),
        };
    }
}

/// <summary>
/// Represents a player in the game.
/// </summary>
public enum Player
{
    One = 1,
    Two = 2,
}

/// <summary>
/// This is the outcome of a hit on a cell.
/// </summary>
public enum GameState
{
    Started = 0,
    Ready = 1,
    Active = 2,
    Complete = 3,
}
