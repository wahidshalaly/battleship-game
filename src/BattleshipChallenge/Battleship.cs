using System.Collections.Generic;

namespace BattleshipChallenge
{
    /// <summary>
    /// This entity represent a ship, its positions, and any damages it receives.
    /// </summary>
    internal class Battleship
    {
        public int Id { get; }
        public List<string> Positions { get; }
        public List<string> Damages { get; }

        public Battleship(int id, IEnumerable<string> positions)
        {
            Id = id;
            Positions = new List<string>(positions);
            Damages = new List<string>();
        }

        /// <summary>
        /// Take an attack at designated position
        /// </summary>
        /// <param name="position">position of attack, expected to be like A5, C6, etc.</param>
        public void AttackAt(string position)
        {
            Damages.Add(position);
        }

        /// <summary>
        /// Returns true if all positions are damaged, and false otherwise.
        /// </summary>
        public bool Sunk => Positions.Count == Damages.Count;
    }
}
