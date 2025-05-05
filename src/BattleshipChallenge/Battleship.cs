using System.Collections.Generic;

namespace BattleshipChallenge;

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Battleship
{
    public int Id { get; }
    public List<string> Cells { get; }
    public List<string> Damages { get; }

    public Battleship(int id, IEnumerable<string> cells)
    {
        Id = id;
        Cells = new List<string>(cells);
        Damages = new List<string>();
    }

    /// <summary>
    /// Takes an attack at designated cell
    /// </summary>
    /// <param name="cell">cell of attack, expected to be like A5, C6, etc.</param>
    public void AttackAt(string cell)
    {
        Damages.Add(cell);
    }

    /// <summary>
    /// Returns true if all cells are damaged, and false otherwise.
    /// </summary>
    public bool Sunk => Cells.Count == Damages.Count;
}