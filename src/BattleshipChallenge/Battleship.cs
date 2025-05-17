using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge;

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Battleship
{
    private readonly List<Cell> _cells;

    public int Id { get; }
    public IEnumerable<Cell> Cells => _cells.AsReadOnly();

    public Battleship(int id, IEnumerable<Cell> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);

        Id = id;

        _cells = [..cells];
    }

    /// <summary>
    /// Takes an attack at designated cell
    /// </summary>
    /// <param name="cell">cell of attack</param>
    public void AttackAt(Cell cell)
    {
        ArgumentNullException.ThrowIfNull(cell);

        if (!_cells.Contains(cell))
        {
            throw new ArgumentException(ErrorMessages.InvalidCellToHit);
        }

        cell.Attack();
    }

    /// <summary>
    /// Returns true if all cells are damaged, and false otherwise.
    /// </summary>
    public bool Sunk => _cells.All(c => c.IsHit);
}