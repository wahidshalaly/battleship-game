using System;

namespace BattleshipChallenge;

/// <summary>
/// This represents an instance of Battleship game, and it tracks the state of the game.
/// </summary>
/// <remarks>
/// Requirements:
/// - Create a board
/// - Add a battleship to a board
/// - Take a hit and report outcome
/// - Return player state
/// - Application should implement only the state tracker, not the entire game
///
/// Analysis:
/// - A game has two players (we'll focus on a single player, but the other won't be different anyway)
/// - When a player joins, create his/her empty board (N*N) where N=10 in this implementation
/// - A player has number of ships (as long as it's possible to add them to the board)
/// - A battleship has a size between 1 and N celled either horizontally or vertically and totally on the board
/// - To take a hit, you need to pick a cell (optionally: that has not been targeted before)
/// - After taking a hit, we must report the state of targeted cell, if it's a hit or not.
/// - This should affect the state of battleship affected & also the state of the board
/// - At any time, we should be able to query the player/board state (if it has lost or not yet)
///
/// Notes:
/// There is no need for a Player entity, it's actually the board state is what matters.
/// If we let the board tracks the battleships on it, then it can report its state based on their states.
/// Q1: When a cell takes a hit, who should report that it's a hit? the cell or battleship in cell?
/// A1: If cell belongs to a battleship, we'll consider it a hit & update battleship state
/// If cell does not have a battleship, we'll consider it a miss.
/// Optionally, in both scenarios, we can update the board state to reject taking hits in this cell again. (BoardCell.CanTakeHit?)
///
/// Outside of scope:
/// - Size of board is fixed to 10 (BOARD_SIZE)
/// </remarks>
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
        switch (player)
        {
            case Player.One:
                return Player1;
            case Player.Two:
                return Player2;
            default:
                throw new ArgumentOutOfRangeException(nameof(player), player, null);
        }
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