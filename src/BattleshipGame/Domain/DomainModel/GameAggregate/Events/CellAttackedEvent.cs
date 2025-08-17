using BattleshipGame.Domain.DomainModel.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate.Events;

/// <summary>
/// Domain event raised when a cell is attacked during gameplay.
/// </summary>
/// <remarks>
/// Initializes a new instance of the CellAttackedEvent class.
/// </remarks>
/// <param name="boardId">The board identifier.</param>
/// <param name="cellCode">The attacked cell code.</param>
/// <param name="cellState">The cell cellState after attack</param>
public class CellAttackedEvent(
    GameId boardId,
    string cellCode,
    CellState cellState) : DomainEvent<CellAttackedEvent>
{
    /// <summary>
    /// Gets the game identifier.
    /// </summary>
    public GameId BoardId { get; } = boardId;

    /// <summary>
    /// Gets the attacked cell code.
    /// </summary>
    public string CellCode { get; } = cellCode;

    /// <summary>
    /// Gets a value indicating whether the attack was a hit.
    /// </summary>
    public CellState CellState { get; } = cellState;
}
