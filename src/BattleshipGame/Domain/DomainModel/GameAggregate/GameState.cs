namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// This is the outcome of a hit on a cell.
/// </summary>
public enum GameState
{
    None = 0,
    Started = 1,
    BoardsAreReady = 2,
    GameOn = 3,
    GameOver = 4,
}
