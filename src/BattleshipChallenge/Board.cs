using System;
using System.Collections.Generic;

namespace BattleshipChallenge
{
    internal class Board
    {
        public const int BoardSize = 10;

        public Dictionary<string, int?> Positions { get; private set; }
        public HashSet<Battleship> Ships { get; private set; }
        public HashSet<string> Attacks { get; private set; }

        public Board()
        {
            // TODO: initialise the board with all positions
        }

        public void AddShip(string bow, string stern)
        {
            // TODO: Add a ship to mentioned positions and mark these positions are occupied by ship-id
            // TODO: Validate that ship is positioned either horizontally or vertically
            throw new NotImplementedException();
        }
        public bool Attack(string position)
        {
            // TODO: find ship-id if any and send it the attack and return Hit.
            // if not ship-id found, then it's a Miss.
            // return true as hit and false as miss
            // TODO: add this attack to attacks collection and it should be attacked again.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true when all ships are sunk.
        /// </summary>
        public bool IsGameOver => throw new NotImplementedException();
    }
}