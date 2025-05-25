using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BattleshipChallenge;

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Ship
{
    /// <summary>
    /// Gets the unique identifier for this ship
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the type of this ship
    /// </summary>
    public ShipKind Kind { get; }

    public ReadOnlyCollection<string> Position { get; }

    /// <summary>
    /// Creates a new battleship with the specified ID, type, and cells
    /// </summary>
    /// <param name="id">The unique identifier for this ship</param>
    /// <param name="kind">The type of ship</param>
    /// <param name="position">The ship position, cells it occupies</param>
    public Ship(int id, ShipKind kind, List<string> position)
    {
        ValidatePosition(kind, position);

        Id = id;
        Kind = kind;
        Position = new ReadOnlyCollection<string>(position);
    }

    private static void ValidatePosition(ShipKind kind, List<string> position)
    {
        ArgumentNullException.ThrowIfNull(position);

        if (position.Count != (int)kind)
        {
            throw new ApplicationException(ErrorMessages.InvalidShipPosition_Count);
        }

        var cells = position.Select(c => new Cell(c)).ToList();

        if (AllHasSameColumn())
        {
            var start = cells[0].Digit;
            if (cells.Any(cell => cell.Digit != start++))
            {
                throw new ApplicationException(ErrorMessages.InvalidShipPosition_Alignment);
            }
        }
        else if (AllHasSameRow())
        {
            var start = Constants.Alphabet.IndexOf(cells[0].Letter);
            if (cells.Any(cell => Constants.Alphabet.IndexOf(cell.Letter) != start++))
            {
                throw new ApplicationException(ErrorMessages.InvalidShipPosition_Alignment);
            }
        }
        else
        {
            throw new ApplicationException(ErrorMessages.InvalidShipPosition_Alignment);
        }

        return;

        bool AllHasSameColumn() => cells.All(c => c.HasSameColumn(cells[0]));
        bool AllHasSameRow() => cells.All(c => c.HasSameRow(cells[0]));
    }
}
