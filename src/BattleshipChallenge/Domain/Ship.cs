using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BattleshipChallenge.Common;
using BattleshipChallenge.Domain.Base;

namespace BattleshipChallenge.Domain;

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Ship : Entity<int>
{
    private readonly HashSet<string> _hits = [];

    /// <summary>
    /// Gets the type of this ship
    /// </summary>
    public ShipKind Kind { get; }

    /// <summary>
    /// Ship's position, cells it occupies
    /// </summary>
    public ReadOnlyCollection<string> Position { get; }

    public bool Sunk { get; private set; }

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

    public void Attack(string code)
    {
        if (!Position.Contains(code))
        {
            throw new NotImplementedException();
        }

        _hits.Add(code);

        if (_hits.Count == Kind.ToSize())
        {
            Sunk = true;
        }
    }

    private static void ValidatePosition(ShipKind kind, List<string> position)
    {
        ArgumentNullException.ThrowIfNull(position);

        if (position.Count != kind.ToSize())
        {
            throw new ApplicationException(ErrorMessages.InvalidShipPosition_Count);
        }

        var cells = position.Select(Cell.FromCode).ToList();

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

        bool AllHasSameColumn() => cells.All(c => c.Letter == cells[0].Letter);
        bool AllHasSameRow() => cells.All(c => c.Digit == cells[0].Digit);
    }
}
