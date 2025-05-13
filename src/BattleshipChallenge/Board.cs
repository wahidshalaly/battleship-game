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

    // All cells on the board and their corresponding ShipId, if any
    public Dictionary<Cell, int?> Cells { get; }
    public List<Battleship> Ships { get; }
    public List<Cell> Attacks { get; }

    public Board(ICellLocator locator, int size)
    {
        _locator = locator;

        Cells = new Dictionary<Cell, int?>();
        Ships = [];
        Attacks = [];

        var cells = _locator
            .GetAllCellsOnBoardOf(_boardSize)
            .ToList();

        cells.ForEach(p => Cells.Add(p, null));
    }

    public Board(ICellLocator locator)
    {
        _locator = locator;

        Cells = new Dictionary<Cell, int?>();
        Ships = [];
        Attacks = [];

        var cells = _locator
            .GetAllCellsOnBoardOf(_boardSize)
            .ToList();

        cells.ForEach(p => Cells.Add(p, null));
    }

    public Board(int size)
    {
        if (size is <= 0 or > Constants.MaxBoardSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, Constants.ErrorMessages.InvalidBoardSize);
        }

        _boardSize = size;
    }

    /// <summary>
    /// Returns true when all ships are sunk.
    /// </summary>
    public bool IsGameOver => Ships.All(s => s.Sunk);

    public void AddShip(Cell bow, Cell stern)
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
    /// <exception cref="ArgumentException">If an invalid cell, it'll throw an Argument exception</exception>
    public bool Attack(Cell cell)
    {
        if (cell == null)
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

    private IEnumerable<Cell> FindShipLocation(Cell bow, Cell stern)
    {
        if (!Cells.ContainsKey(bow) || !Cells.ContainsKey(stern))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidCellOutOfRange);
        }

        if (!bow.HasSameColumn(stern) && !bow.HasSameRow(stern))
        {
            throw new ArgumentException(Constants.ErrorMessages.InvalidShipLocation);
        }

        return _locator.FindCellsBetween(bow, stern);
    }
}