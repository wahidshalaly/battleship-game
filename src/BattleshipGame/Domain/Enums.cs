namespace BattleshipGame.Domain;

/// <summary>
/// This represents the states of a cell.
/// Clear, when it's not assigned a ship and hasn't been targeted before.
/// Occupied, when it's assigned to a ship.
/// Hit, when it's been targeted before.
/// </summary>
public enum CellState
{
    Clear,
    Occupied,
    Hit,
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
    Destroyer = 0,
    Cruiser = 1,
    Submarine = 2,
    Battleship = 3,
    Carrier = 4,
}

/// <summary>
/// This represents the orientation of a ship on the board, either vertical or horizontal.
/// </summary>
public enum ShipOrientation
{
    Vertical,
    Horizontal,
}

/// <summary>
/// Represents a player in the game.
/// </summary>
public enum Player
{
    Own = 1,
    Opp = 2,
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
