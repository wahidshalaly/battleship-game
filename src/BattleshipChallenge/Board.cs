using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipChallenge;

/// <summary>
/// This entity represents a board, its cells and ships it contains.
/// It keeps track of attacks and can query ships for their state.
/// The state of board <c>IsGameOver</c> will be <c>true</c>, if all ships are sunk.
/// </summary>
internal class Board
{
    public const int DefaultSize = 10;
    public const int MaximumSize = 26;

    private static readonly List<char> _letters = Constants.Alphabet.ToList();

    private readonly int _boardSize;
    private readonly Dictionary<string, Cell> _cells = new();
    private readonly List<Ship> _ships = [];

    public IList<Cell> Cells => _cells.Values.ToList();
    public IList<Ship> Ships => _ships.AsReadOnly();

    public Board(int size = DefaultSize)
    {
        if (size is < DefaultSize or > MaximumSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidBoardSize);
        }

        _boardSize = size;

        GenerateBoardCells();
    }

    /// <summary>
    /// Returns true when all ships are sunk.
    /// </summary>
    public bool IsGameOver => _ships.All(s => s.Sunk);

    /// <summary>
    /// Adds a ship to the board between the specified bow and stern cells.
    /// Validates that the ship follows standard Battleship rules and doesn't overlap with existing ships.
    /// </summary>
    /// <param name="shipKind"></param>
    /// <param name="bowCode">The bow position of the ship</param>
    /// <param name="orientation">The orientation of the ship</param>
    /// <exception cref="ArgumentException">Thrown when the ship placement is invalid</exception>
    public void AddShip(ShipKind shipKind, string bowCode, ShipOrientation orientation)
    {
        ValidateShipPosition(shipKind, bowCode, orientation, out var bow, out var stern);

        var shipId = _ships.Count + 1;
        var cells = GetShipCells(bow, stern).ToList();
        foreach (var cell in cells)
        {
            cell.Assign(shipId);
        }

        var position = cells.Select(c => c.Code).ToList();
        var ship = new Ship(shipId, shipKind, position);
        _ships.Add(ship);
    }

    /// <summary>
    /// Attacks a cell on the board.
    /// </summary>
    /// <param name="code">A valid cell on the board</param>
    /// <exception cref="ArgumentException">If an invalid cell, it'll throw an Argument exception</exception>
    public void Attack(string code)
    {
        var cell = ValidateAttackPosition(code);
        if (cell.State == CellState.Occupied)
        {
            Ships.First(s => s.Id == cell.ShipId).Attack(code);
        }
        cell.Attack();
    }

    private Cell ValidateAttackPosition(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        if (!_cells.TryGetValue(code, out var cell))
        {
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);
        }

        if (cell.State == CellState.Hit)
        {
            throw new ArgumentException(ErrorMessages.InvalidCellToHit);
        }

        return cell;
    }

    private void GenerateBoardCells()
    {
        for (var i = 0; i < _boardSize; i++)
        {
            for (var j = 1; j <= _boardSize; j++)
            {
                var cell = new Cell(_letters[i], j);
                _cells.Add(cell.Code, cell);
            }
        }
    }

    private void ValidateShipPosition(
        ShipKind shipKind,
        string bowCode,
        ShipOrientation orientation,
        out Cell bow,
        out Cell stern
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bowCode);

        var shipSize = (int)shipKind;
        var bowIsValid = _cells.TryGetValue(bowCode, out bow);
        if (!bowIsValid)
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);

        // Calculate the stern cell based on ship size and orientation
        var sternCode = orientation == ShipOrientation.Vertical
            ? $"{bow.Letter}{bow.Digit + shipSize - 1}"
            : $"{_letters[_letters.IndexOf(bow.Letter) + shipSize - 1]}{bow.Digit}";

        var sternIsValid = _cells.TryGetValue(sternCode, out stern);
        if (!sternIsValid)
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);
    }

    private IEnumerable<Cell> GetShipCells(Cell bow, Cell stern)
    {
        if (bow == stern)
        {
            yield return bow;
            yield break;
        }

        if (bow.Letter == stern.Letter)
        {
            var minDigit = Math.Min(bow.Digit, stern.Digit);
            var maxDigit = Math.Max(bow.Digit, stern.Digit);

            for (var digit = minDigit; digit <= maxDigit; digit++)
            {
                yield return _cells[$"{bow.Letter}{digit}"];
            }
        }
        else
        {
            var minLetterIndex = Math.Min(
                _letters.IndexOf(bow.Letter),
                _letters.IndexOf(stern.Letter)
            );
            var maxLetterIndex = Math.Max(
                _letters.IndexOf(bow.Letter),
                _letters.IndexOf(stern.Letter)
            );

            for (var idx = minLetterIndex; idx <= maxLetterIndex; idx++)
            {
                yield return _cells[$"{_letters[idx]}{bow.Digit}"];
            }
        }
    }
}
