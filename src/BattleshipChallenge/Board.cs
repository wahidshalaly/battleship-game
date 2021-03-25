using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge
{
    internal class Board
    {
        private readonly ILocationTranslator _locationTranslator;

        public static class Constants
        {
            public const int BoardSize = 10;
            public const string ErrorMsg_InvalidPositionOutOfRange = "Selected position must be defined on the board";
            public const string ErrorMsg_InvalidShipPosition = "Ship must be positioned either vertically or horizontally";
        }

        public Dictionary<string, int?> Positions { get; }
        public List<Battleship> Ships { get; }
        public List<string> Attacks { get; }

        public Board(ILocationTranslator locationTranslator)
        {
            _locationTranslator = locationTranslator;

            Positions = new Dictionary<string, int?>();
            Ships = new List<Battleship>();
            Attacks = new List<string>();

            var positions = _locationTranslator.GetAllPositionsOnBoardOf(Constants.BoardSize).ToList();
            positions.ForEach(p => Positions.Add(p, null));
        }

        /// <summary>
        /// Returns true when all ships are sunk.
        /// </summary>
        public bool IsGameOver => throw new NotImplementedException();

        public void AddShip(string bow, string stern)
        {
            var id = Ships.Count;
            var positions = FindShipLocation(bow, stern).ToList();
            var ship = new Battleship(id, positions);
            positions.ForEach(l => Positions[l] = id);
            Ships.Add(ship);
        }

        public bool Attack(string position)
        {
            if (string.IsNullOrWhiteSpace(position))
            {
                throw new ArgumentException(Constants.ErrorMsg_InvalidPositionOutOfRange);
            }

            if (!Positions.ContainsKey(position))
            {
                throw new ArgumentException(Constants.ErrorMsg_InvalidPositionOutOfRange);
            }

            if (!Positions[position].HasValue)
            {
                return false;
            }

            if (Attacks.Contains(position))
            {
                // TODO: Discuss if we should throw an exception when position has been hit more than once and how to prevent this
                return false;
            }

            Attacks.Add(position);
            var shipId = Positions[position].Value;
            Ships[shipId].AttackAt(position);
            return true;
        }

        private IEnumerable<string> FindShipLocation(string bow, string stern)
        {
            if (!Positions.ContainsKey(bow) || !Positions.ContainsKey(stern))
            {
                throw new ArgumentException(Constants.ErrorMsg_InvalidPositionOutOfRange);
            }

            if (bow[0] != stern[0] && bow.Substring(1) != stern.Substring(1))
            {
                throw new ArgumentException(Constants.ErrorMsg_InvalidShipPosition);
            }

            return _locationTranslator.FindPositions(bow, stern);
        }
    }
}