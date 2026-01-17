namespace BattleshipGame.Application.Common;

/// <summary>
/// Represents the current state of a game for AI decision-making.
/// </summary>
public record GameStateContext
{
    /// <summary>
    /// The size of the board (e.g., 10 for 10x10).
    /// </summary>
    public int BoardSize { get; init; }

    /// <summary>
    /// Cells that are valid targets for the next attack.
    /// </summary>
    public List<string> NextTargets { get; init; } = [];

    /// <summary>
    /// Cells that were attacked and hit a ship.
    /// </summary>
    public List<string> Hits { get; init; } = [];

    /// <summary>
    /// Cells that were attacked but missed.
    /// </summary>
    public List<string> Misseds { get; init; } = [];

    /// <summary>
    /// Sizes of remaining ships that haven't been sunk.
    /// Standard Battleship: [5, 4, 3, 3, 2]
    /// </summary>
    public List<int> RemainingShipSizes { get; init; } = [];

    /// <summary>
    /// Game progress: "Starting", "Active", "Won", "Lost", etc.
    /// </summary>
    public string GamePhase { get; init; } = "Active";

    /// <summary>
    /// Number of total ships sunk by AI opponent.
    /// </summary>
    public int ShipsSunk { get; init; }

    /// <summary>
    /// Recent hit cells (last 3) for pattern detection.
    /// Empty if no recent hits.
    /// </summary>
    public List<string> RecentHits { get; init; } = [];
}
