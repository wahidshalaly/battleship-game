using System;
using System.Collections.Generic;

namespace BattleshipChallenge
{
    /// <summary>
    /// This will be the state tracker of the Battleship game
    /// </summary>
    /// <remarks>
    /// Requirements:
    /// - Create a board
    /// - Add a battleship to a board
    /// - Take a hit and report outcome
    /// - Return player state
    /// - Application should implement only the state tracker, not the entire game
    ///
    /// Analysis:
    /// - A game has two players (we'll focus on a single player, but the other won't be different anyway)
    /// - When a player joins, create his/her empty board (N*N) where N=10 in this implementation
    /// - A player has number of ships (as long as it's possible to add them to the board)
    /// - A battleship has a size (s) 1 <= s <= N positioned either horizontally or vertically and totally on the board
    /// - To take a hit, you need to pick a position (optionally: that has not been targetted before)
    /// - After taking a hit, we must report the state of targetted position, if it's a hit or not.
    /// - This should affect the state of battleship affected & also the state of the board
    /// - At any time, we should be able to query the player/board state (if has lost or not yet)
    ///
    /// Notes:
    /// There is no need for a Player entity, it's actually the board state is what matters.
    /// If we let the board tracks the battleships on it, then it can report its state based on their states.
    /// Q1: When a position takes a hit, who should report that it's a a hit? the position or battleship in position?
    /// A1: If position belongs to a battleship, we'll consider it a hit & update battleship state
    /// If position does not have a battleship, we'll consider it a miss.
    /// Optionally, in both scenarios, we can update the board state to reject taking hits in this position again. (BoardPosition.CanTakeHit?)
    ///
    ///
    /// Outside of scope:
    /// - Size of board is fixed to 10 (BOARD_SIZE)
    /// - No support for different battleship types, only validate that ship is positioned correctly on the board
    /// </remarks>
    public class Game
    {
        private Board Player1 { get; set; }
        private Board Player2 { get; set; }

        public void CreateBoard(Player player)
        {
            // TODO: Initialise a board with all positions
            // validate to create board for a player only once
            // Later to think about when game is over, how to re-initialise the board
            throw new NotImplementedException();
        }

        public void AddShip(Player player, string bow, string stern)
        {
            // TODO: Send command to player's board
            throw new NotImplementedException();
        }

        public bool Attack(Player player, string position)
        {
            // TODO: Send command to player's board
            throw new NotImplementedException();
        }
    }

    public enum Player { One = 1, Two = 2 }

    internal class Board
    {
        public const int BoardSize = 10;

        private HashSet<Battleship> _ships;
        private Dictionary<string, Guid?> _positions;
        private HashSet<string> _attacks;

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

    internal class Battleship
    {
        private Guid _id;
        private HashSet<string> _positions;
        private HashSet<string> _damages;

        public Battleship(Guid id, string bow, string stern)
        {
        }

        public void Attack(string position)
        {
            // TODO: mark hit position as damaged
            throw new NotImplementedException();
        }

        public bool Sunk => _positions.Count == _damages.Count;
    }
}
