using System.Collections.Generic;

namespace BattleshipChallenge;

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Battleship
{
    public int Id { get; }
    public List<Cell> Cells { get; }
    public List<Cell> Damages { get; }

    public Battleship(int id, IEnumerable<Cell> cells)
    {
        Id = id;
        Cells = [..cells];
        Damages = [];
    }

    /// <summary>
    /// Takes an attack at designated cell
    /// </summary>
    /// <param name="cell">cell of attack</param>
    public void AttackAt(Cell cell)
    {
        Damages.Add(cell);
    }

    /// <summary>
    /// Returns true if all cells are damaged, and false otherwise.
    /// </summary>
    public bool Sunk => Cells.Count == Damages.Count;
}