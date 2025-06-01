using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Common;
using BattleshipGame.Domain.ValueObjects;

namespace BattleshipGame.Domain.Entities;

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Ship : Entity<int>
{
    private readonly HashSet<string> _cells;
    private readonly HashSet<string> _hits = [];

    /// <summary>
    /// Gets the type of this ship
    /// </summary>
    public ShipKind Kind { get; }

    /// <summary>
    /// Ship's position, cells it occupies
    /// </summary>
    public IReadOnlyCollection<string> Position => _cells;

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
        _cells = new(position);
    }

    public void Attack(string code)
    {
        if (!_cells.Contains(code))
        {
            throw new ApplicationException(ErrorMessages.InvalidShipAttack);
        }

        if (!_hits.Add(code))
        {
            throw new ApplicationException(ErrorMessages.InvalidCellToAttack);
        }

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
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].Digit != cells[0].Digit + i)
                {
                    throw new ApplicationException(ErrorMessages.InvalidShipPosition_Alignment);
                }
            }
        }
        else if (AllHasSameRow())
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].Letter != cells[0].Letter + i)
                {
                    throw new ApplicationException(ErrorMessages.InvalidShipPosition_Alignment);
                }
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
