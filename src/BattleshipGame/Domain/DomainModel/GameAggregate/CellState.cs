namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// This represents the states of a cell.
/// Clear, when it's not assigned a ship and hasn't been targeted before.
/// Occupied, when it's assigned to a ship.
/// Hit, when it's been targeted before.
/// </summary>
public enum CellState
{
    None = 0,
    Clear,
    Occupied,
    Hit,
    Missed,
}