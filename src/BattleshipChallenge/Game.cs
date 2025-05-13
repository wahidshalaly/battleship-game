using System;

namespace BattleshipChallenge;

/// <summary>
/// This represents an instance of Battleship game, and it tracks the state of the game.
/// </summary>
public class Game
{
    private Board Player1 { get; set; }
    private Board Player2 { get; set; }

    public void CreateBoard(Player player)
    {
        switch (player)
        {
            case Player.One:
                Player1 = CreateNewBoard();
                break;
            case Player.Two:
                Player2 = CreateNewBoard();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player), player, null);
        }
    }

    public void AddShip(Player player, Cell bow, Cell stern)
    {
        BoardSelector(player).AddShip(bow, stern);
    }

    public bool Attack(Player player, Cell cell)
    {
        return BoardSelector(player).Attack(cell);
    }

    public bool PlayerHasLost(Player player) => BoardSelector(player).IsGameOver;

    private readonly CellLocator _cellLocator = new();

    private Board CreateNewBoard()
    {
        return new Board(_cellLocator);
    }

    private Board BoardSelector(Player player)
    {
        return player switch
        {
            Player.One => Player1,
            Player.Two => Player2,
            _ => throw new ArgumentOutOfRangeException(nameof(player), player, null)
        };
    }
}

public enum Player
{
    One = 1,
    Two = 2,
}

/// <summary>
/// These are the types of ships that can be placed on the board and their sizes.
/// Destroyer: 2 cells
/// Submarine: 3 cells
/// Cruiser: 3 cells
/// Battleship: 4 cells
/// Carrier: 5 cells
/// </summary>
public enum ShipKind
{
    Destroyer = 2,
    Cruiser = 3,
    Submarine = 3,
    Battleship = 4,
    Carrier = 5,
}

/// <summary>
/// This is the outcome of a hit on a cell.
/// </summary>
public enum HitResult
{
    Miss = 0,
    Hit = 1,
    Sunk = 2,
}