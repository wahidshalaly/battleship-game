namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// This is the outcome of a hit on a cell.
/// </summary>
public enum GameState
{
    None = 0,
    New = 1,
    Ready = 2,
    Started = 3,
    GameOver = 4,
}
