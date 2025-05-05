using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge;

/// <summary>
/// This entity represents a board, its cells and ships it contains.
/// It keeps track of attacks and can query ships for their state.
/// The state of board <c>IsGameOver</c> will be <c>true</c>, if all ships are sunk.
/// This board has a fixed size of 10.
/// </summary>
internal class Board
{
    private readonly int _boardSize = 10;
    private readonly ICellLocator _locator;

    // All cell codes on the board and their corresponding ShipId, if any
    public Dictionary<string, int?> Cells { get; }
    public List<Battleship> Ships { get; }
    public List<string> Attacks { get; }

    public Board(ICellLocator locator)
    {
        _locator = locator;

        Cells = new Dictionary<string, int?>();
        Ships = new List<Battleship>();
        Attacks = new List<string>();

        var cells = _locator
            .GetAllCellsOnBoardOf(_boardSize)
            .ToList();
        cells.ForEach(p => Cells.Add(p, null));
    }

    public Board(int size)
    {
        if (size <= 0 || size > Constants.MaxBoardSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, Constants.ErrorMessages.InvalidBoardSize);
        }

        _boardSize = size;
    }

    /// <summary>
    /// Returns true when all ships are sunk.
    /// </summary>
    public bool IsGameOver => Ships.All(s => s.Sunk);

    public void AddShip(string bow, string stern)
    {
        var shipId = Ships.Count;
        var cells = FindShipLocation(bow, stern).ToList();
        var ship = new Battleship(shipId, cells);
        cells.ForEach(c => Cells[c] = shipId);
        Ships.Add(ship);
    }

    /// <summary>
    /// Attacks a cell on the board.
    /// </summary>
    /// <param name="cell">A valid cell on the board</param>
    /// <returns>True, if a `Hit`, false if a `Miss`</returns>
    /// <exception cref="ArgumentException">If not a valid cell it'll throw an Argument exception</exception>
    public bool Attack(string cell)
    {
        if (string.IsNullOrWhiteSpace(cell))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellCode);
        }

        if (!Cells.TryGetValue(cell, out var shipId))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellOutOfRange);
        }

        if (!shipId.HasValue)
        {
            return false;
        }

        if (Attacks.Contains(cell))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellToHit);
        }

        Attacks.Add(cell);
        Ships[shipId.Value].AttackAt(cell);
        return true;
    }

    private IEnumerable<string> FindShipLocation(string bow, string stern)
    {
        if (!Cells.ContainsKey(bow) || !Cells.ContainsKey(stern))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellOutOfRange);
        }

        var bowCell = (Cell)bow;
        var sternCell = (Cell)stern;
        if (!bowCell.HasSameColumn(sternCell) && !bowCell.HasSameRow(sternCell))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidShipLocation);
        }

        return _locator.FindCells(bow, stern);
    }
}