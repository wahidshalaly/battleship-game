using BattleshipGame.Domain.Common;
using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Represents the unique identifier for a ship.
/// </summary>
/// <remarks>This type is a strongly-typed identifier for ships, encapsulating a <see cref="Guid"/> value. It is
/// used to ensure type safety and clarity when working with ship-related entities.</remarks>
/// <param name="Value"></param>
public record ShipId(Guid Value) : EntityId(Value);

/// <summary>
/// This represents a ship on the board, its cells, and any damages it receives.
/// </summary>
internal class Ship : Entity<ShipId>
{
    private readonly HashSet<string> _cells;
    private readonly HashSet<string> _hits;

    /// <summary>
    /// Gets the type of this ship
    /// </summary>
    public ShipKind Kind { get; }

    /// <summary>
    /// Ship's position, cells it occupies
    /// </summary>
    public IReadOnlyCollection<string> Position => _cells;

    public bool Sunk => _cells.Count == _hits.Count;

    /// <summary>
    /// Creates a new battleship with the specified ID, type, and cells
    /// </summary>
    /// <param name="kind">The type of ship</param>
    /// <param name="position">The ship position, cells it occupies</param>
    public Ship(ShipKind kind, List<string> position)
    {
        ValidatePosition(kind, position);

        Kind = kind;
        _cells = [.. position];
        _hits = [];
    }

    public void Attack(string code)
    {
        if (!_cells.Contains(code))
        {
            throw new InvalidOperationException(ErrorMessages.InvalidShipAttack);
        }

        if (!_hits.Add(code))
        {
            throw new InvalidOperationException(ErrorMessages.InvalidCellToAttack);
        }
    }

    private static void ValidatePosition(ShipKind kind, List<string> position)
    {
        ArgumentNullException.ThrowIfNull(position);

        if (position.Count != kind.ToSize())
        {
            throw new InvalidOperationException(ErrorMessages.InvalidShipPosition_Count);
        }

        var cells = position.Select(Cell.FromCode).ToList();

        if (AllHasSameColumn())
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].Digit != cells[0].Digit + i)
                {
                    throw new InvalidOperationException(ErrorMessages.InvalidShipPosition_Alignment);
                }
            }
        }
        else if (AllHasSameRow())
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].Letter != cells[0].Letter + i)
                {
                    throw new InvalidOperationException(ErrorMessages.InvalidShipPosition_Alignment);
                }
            }
        }
        else
        {
            throw new InvalidOperationException(ErrorMessages.InvalidShipPosition_Alignment);
        }

        return;

        bool AllHasSameColumn() => cells.All(c => c.Letter == cells[0].Letter);
        bool AllHasSameRow() => cells.All(c => c.Digit == cells[0].Digit);
    }
}
