using System;
using System.Collections.Generic;

namespace BattleshipChallenge
{
    /// <summary>
    /// Simple data structure that encapsulates data of a ship
    /// It's ID, positions, and any damages it receives.
    /// No need for validating if initial positions are either horizontally or virtually
    /// Or, if positions of attack actually belong to ship or not, this all will be responsibility of the Board
    /// </summary>
    internal class Battleship
    {
        public int Id { get; }
        public List<string> Positions { get; }
        public List<string> Damages { get; }

        public Battleship(int id, List<string> positions)
        {
            Id = id;
            Positions = positions;
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