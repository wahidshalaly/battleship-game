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
            - You opponent's board has 5 ships of sizes 5, 4, 3, 3, and 2 cells.
            - They're either placed horizontally or vertically without overlapping.

            Always respond with valid JSON containing "cell" and "reasoning" fields.
            The cell must be a valid board position within the board rage (A1 through J10, or equivalent for larger boards).
            Keep reasoning brief but strategic.
            """;
    }

    /// <inheritdoc />
    public string BuildStrategicPrompt(GameSnapshot context)
    {
        var hitDisplay =
            context.Hits.Count > 0 ? string.Join(", ", context.Hits.OrderBy(c => c)) : "None";

        var missDisplay =
            context.Misses.Count > 0 ? string.Join(", ", context.Misses.OrderBy(c => c)) : "None";

        var boardDescription = context.BoardDescription;

        return $$"""
            Select your next attack cell based on the following game context.

            BOARD: {{boardDescription}}

            ATTACK HISTORY:
            - HITS: {{hitDisplay}}
            - MISSES: {{missDisplay}}

            VALID TARGETS: Any cell NOT in the attack history above



            STRATEGY TIPS:
            1. If you have HITS with unknown adjacent cells (up/down/left/right only, NOT diagonal), attack an adjacent cell
            2. If multiple HITS form a line, continue in that direction until you miss or sink
            3. Otherwise, target cells with highest probability (center area, checkerboard pattern)
            4. Never attack previously attacked cells or invalid coordinates

            OUTPUT FORMAT:
            Respond with ONLY raw JSON (no markdown, no code blocks, no backticks):

            Example: 
                {"cell": "B3", "reasoning": "Adjacent to hit at C3 to determine ship orientation"}
            Or

            """;
    }
}
