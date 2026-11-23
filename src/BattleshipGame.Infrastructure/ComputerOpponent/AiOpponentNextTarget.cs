namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Represents the LLM's response for selecting a target cell.
/// </summary>
/// <param name="Cell">The selected cell (e.g., "B4").</param>
/// <param name="Reasoning">The reasoning behind the selection.</param>
public sealed record AiOpponentNextTarget(string Cell, string? Reasoning);
