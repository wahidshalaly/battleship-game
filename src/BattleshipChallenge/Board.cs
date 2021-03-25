using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge
{
    internal class Board
    {
        private readonly ILocationTranslator _locationTranslator;

        public const int BoardSize = 10;

        public Dictionary<string, int?> Positions { get; }
        public List<Battleship> Ships { get; }
        public List<string> Attacks { get; }

        public Board(ILocationTranslator locationTranslator)
        {
            _locationTranslator = locationTranslator;

            Positions = new Dictionary<string, int?>();
            Ships = new List<Battleship>();
            Attacks = new List<string>();

            var positions = _locationTranslator.GetAllPositionsOnBoardOf(BoardSize);
            positions.ToList().ForEach(p => Positions.Add(p, null));
        }

        /// <summary>
        /// Returns true when all ships are sunk.
        /// </summary>
        public bool IsGameOver => throw new NotImplementedException();

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
    }
}