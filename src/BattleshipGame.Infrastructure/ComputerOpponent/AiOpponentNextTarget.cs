namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Represents the LLM's response for selecting a target cell.
/// </summary>
public sealed record AiOpponentNextTarget(
    /// <summary>
    /// The selected cell (e.g., "B4").
    /// </summary>
    string Cell,
    /// <summary>
    /// The reasoning behind the selection.
    /// </summary>
    string? Reasoning
);
