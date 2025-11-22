namespace BattleshipGame.Domain.DomainModel.GameAggregate;

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
    None = 0,
    Destroyer = 1,
    Cruiser = 2,
    Submarine = 3,
    Battleship = 4,
    Carrier = 5,
}
