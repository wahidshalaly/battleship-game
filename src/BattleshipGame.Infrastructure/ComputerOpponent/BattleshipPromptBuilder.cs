using BattleshipGame.Application.Common;
using BattleshipGame.Application.Interfaces.ComputerOpponent;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Builds prompts for Battleship game LLM-based strategies.
/// </summary>
public sealed class BattleshipPromptBuilder : IPromptBuilder
{
    /// <inheritdoc />
    public string BuildSystemPrompt()
    {
        return """
            You are an expert Battleship strategist. Your role is to analyze the game board and recommend 
            the optimal cell to attack next.

            You understand:
            - Ship placement patterns and probabilities
            - How to follow up on hits to sink ships efficiently
            - Probability density mapping for ship locations
            - Adapting strategy based on remaining ship sizes

            Always respond with valid JSON containing "cell" and "reasoning" fields.
            The cell must be a valid board position (A1 through J10, or equivalent for larger boards).
            Keep reasoning brief but strategic.
            """;
    }

    /// <inheritdoc />
    public string BuildStrategicPrompt(GameStateContext context)
    {
        var hitDisplay =
            context.Hits.Count > 0 ? string.Join(", ", context.Hits.OrderBy(c => c)) : "None";

        var missDisplay =
            context.Misses.Count > 0 ? string.Join(", ", context.Misses.OrderBy(c => c)) : "None";

        var boardRange = context.BoardRange;

        return $$"""
            BATTLESHIP GAME - SELECT YOUR NEXT ATTACK
            ==========================================

            BOARD RANGE: {{boardRange}} - {{context.BoardSize}} x {{context.BoardSize}} grid

            YOUR ATTACK HISTORY:
            - HITS: {{hitDisplay}}
            - MISSES: {{missDisplay}}

            VALID TARGETS: Any cell from {{boardRange}} that is NOT listed above.

            STRATEGY TIPS:
            - If you have HITS, attack adjacent cells (up/down/left/right) to sink the ship
            - Multiple hits in a line indicate ship orientation - continue that direction
            - Avoid cells already attacked (listed above)

            RESPOND WITH JSON ONLY (no markdown):
            {"cell": "E5", "reasoning": "brief reason"}
            """;
    }

    /// <inheritdoc />
    public string BuildRetryPrompt(string originalPrompt, IReadOnlyList<string> availableTargets)
    {
        var sampleCells = string.Join(", ", availableTargets.Take(10));
        return $$"""
            {{originalPrompt}}

            Your previous response was invalid. You MUST respond with valid JSON only.
            Example valid cells: {{sampleCells}}

            {"cell": "X", "reasoning": "Y"}
            """;
    }
}
