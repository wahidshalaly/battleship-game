using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.Common;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Represents the unique identifier for a board entity.
/// </summary>
/// <remarks>This type is a value object that encapsulates a <see cref="Guid"/> to uniquely identify a board. It
/// is used to ensure type safety and clarity when working with board-related operations.</remarks>
/// <param name="Value"></param>
public record BoardId(Guid Value) : EntityId(Value);

/// <summary>
/// This entity represents a board, its cells and ships it contains.
/// It keeps track of attacks and can query ships for their state.
/// The state of board <c>IsGameOver</c> will be <c>true</c>, if all ships are sunk.
/// </summary>
internal class Board : Entity<BoardId>
{
    private static readonly List<char> _letters = [.. ColumnHeaders];

    private readonly int _boardSize;
    private readonly Dictionary<string, Cell> _grid = [];
    private readonly List<Ship> _ships = new(ShipAllowance);

    public IList<Cell> Cells => _grid.Values.ToList().AsReadOnly();
    public IList<Ship> Ships => _ships.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether the current state satisfies the readiness condition.
    /// </summary>
    public bool IsReady => _ships.Count == ShipAllowance;

    /// <summary>
    /// Returns true when all ships are sunk.
    /// </summary>
    public bool IsGameOver => _ships.Count > 0 && _ships.All(s => s.Sunk);

    public Board(int size = DefaultBoardSize)
    {
        if (size is < DefaultBoardSize or > MaximumBoardSize)
        {
            throw new ArgumentException(ErrorMessages.InvalidBoardSize);
        }

        _boardSize = size;

        GenerateBoardCells();
    }

    /// <summary>
    /// Adds a ship to the board between the specified bow and stern cells.
    /// Validates that the ship follows standard Battleship rules and doesn't overlap with existing ships.
    /// </summary>
    /// <param name="shipKind"></param>
    /// <param name="orientation">The orientation of the ship</param>
    /// <param name="bowCode">The bow position of the ship</param>
    /// <exception cref="ArgumentException">Thrown when the ship placement is invalid</exception>
    public ShipId AddShip(ShipKind shipKind, ShipOrientation orientation, string bowCode)
    {
        ValidateBeforeAddShip(shipKind, orientation, bowCode, out var bow, out var stern);

        var cells = GetShipCells(bow, stern).ToList();
        var position = cells.Select(c => c.Code).ToList();
        var ship = new Ship(shipKind, position);
        foreach (var cell in cells)
        {
            cell.Assign(ship.Id);
        }

        _ships.Add(ship);

        return ship.Id;
    }

    /// <summary>
    /// Attacks a cell on the board.
    /// </summary>
    /// <param name="code">A valid cell on the board</param>
    /// <exception cref="ArgumentException">If an invalid cell, it'll throw an Argument exception</exception>
    public (CellState, ShipId?, bool) Attack(string code)
    {
        var cell = ValidateBeforeAttack(code);
        var shipSunk = false;

        cell.Attack();

        if (cell.State == CellState.Hit)
        {
            var ship = _ships.First(s => s.Id == cell.ShipId);
            ship.Attack(code);
            shipSunk = ship.Sunk;
        }

        return (cell.State, cell.ShipId, shipSunk);
    }

    private void GenerateBoardCells()
    {
        for (var letterIdx = 0; letterIdx < _boardSize; letterIdx++)
        {
            for (var digit = 1; digit <= _boardSize; digit++)
            {
                var cell = new Cell(_letters[letterIdx], digit);
                _grid.Add(cell.Code, cell);
            }
        }
    }

    private void ValidateBeforeAddShip(
        ShipKind kind,
        ShipOrientation orientation,
        string bowCode,
        out Cell bow,
        out Cell stern
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bowCode);

        var shipSize = kind.ToSize();
        var bowIsValid = _grid.TryGetValue(bowCode, out bow!);
        if (!bowIsValid)
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);

        if (orientation == ShipOrientation.None)
        {
            throw new ArgumentException(ErrorMessages.InvalidShipOrientation);
        }

        // Calculate the stern cell based on ship size and orientation
        var sternCode =
            orientation == ShipOrientation.Vertical
                ? $"{bow.Letter}{bow.Digit + shipSize - 1}"
                : $"{_letters[_letters.IndexOf(bow.Letter) + shipSize - 1]}{bow.Digit}";

        var sternIsValid = _grid.TryGetValue(sternCode, out stern!);
        if (!sternIsValid)
            throw new ArgumentException(ErrorMessages.InvalidShipOnBoardPosition);

        if (_ships.Any(s => s.Kind.ToString() == kind.ToString()))
        {
            throw new InvalidOperationException(ErrorMessages.InvalidShipKindAlreadyExists);
        }

        if (_ships.Count == ShipAllowance)
        {
            throw new InvalidOperationException(ErrorMessages.InvalidShipAddition);
        }
    }

    private Cell ValidateBeforeAttack(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        if (!_grid.TryGetValue(code, out var cell))
        {
            throw new ArgumentException(ErrorMessages.InvalidCellCode);
        }

        return cell;
    }

    private IEnumerable<Cell> GetShipCells(Cell bow, Cell stern)
    {
        if (bow.Code == stern.Code)
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
                yield return _grid[$"{bow.Letter}{digit}"];
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
                yield return _grid[$"{_letters[idx]}{bow.Digit}"];
            }
        }
    }
}
