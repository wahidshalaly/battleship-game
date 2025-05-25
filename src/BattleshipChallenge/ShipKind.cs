using System.Collections.Generic;

namespace BattleshipChallenge;

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

public static class ShipKindExtensions
{
    private static readonly Dictionary<ShipKind, int> _sizes = new()
    {
        { ShipKind.Destroyer, 2 },
        { ShipKind.Cruiser, 3 },
        { ShipKind.Submarine, 3 },
        { ShipKind.Battleship, 4 },
        { ShipKind.Carrier, 5 },
    };

    public static int ToSize(this ShipKind kind) => _sizes[kind];
}
