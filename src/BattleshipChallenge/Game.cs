using System;

namespace BattleshipChallenge
{
    /// <summary>
    /// This will be the state tracker of the Battleship game
    /// </summary>
    /// <remarks>
    /// Requirements:
    /// - Create a board
    /// - Add a battleship (Any number of ships of size <= N)
    /// - Take a hit and report outcome
    /// - Return player status
    /// - Application should implement only the state tracker, not the entire game
    ///
    /// Analysis:
    /// - A game has two players (we'll focus on a single player, but the other won't be different anyway)
    /// - When a player joins, create his/her empty board (N*N) where N=10 in this implementation
    /// - A player has number of ships (as long as it's possible to add them to the board)
    /// - A battleship has a size (s) 1 <= s <= N, positioned either horizontally or vertically, totally on the board
    /// - To take a hit, you need to pick a position (optionally: that has not been targetted before)
    /// - After taking a hit, we must report the state of targetted position, if it's a hit or not. (Battleship.IsHit?)
    /// - This should affect the state of battleship affected & also the state of the board
    /// - At any time, we should be able to query the player/board state (if has lost or not yet) (Board.IsGameOver?)
    ///
    /// Notes:
    /// There is no need for a Player entity, it's actually the board state is what matters.
    /// If we let the board tracks the battleships on it, then it can report its state based on their states.
    /// Q1: When a position takes a hit, who should report that it's a a hit? the position or battleship in position?
    /// A1: If position belongs to a battleship, we'll consider it a hit & update battleship state
    /// If position does not have a battleship, we'll consider it a miss.
    /// Optionally, in both scenarios, we can update the board state to reject taking hits in this position again. (BoardPosition.CanTakeHit?)
    /// </remarks>
    public class Game
    {
    }
}
