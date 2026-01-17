using BattleshipGame.Application.Common;
using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

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
            context.Hits.Count > 0 ? string.Join(", ", context.Hits.OrderBy(c => c)) : "None yet";

        var missDisplay =
            context.Misses.Count > 0
                ? string.Join(", ", context.Misses.OrderBy(c => c))
                : "None yet";

        var totalAttacks = context.Hits.Count + context.Misses.Count;
        var hitRatio = totalAttacks > 0 ? $"{context.Hits.Count * 100 / totalAttacks}%" : "0%";

        var availableSample = string.Join(", ", context.AvailableTargets.Take(15));
        var moreAvailable =
            context.AvailableTargets.Count > 15
                ? $" ...and {context.AvailableTargets.Count - 15} more cells"
                : "";

        var gameStateDisplay = context.GameState switch
        {
            GameState.Started => "Active",
            GameState.Ready => "Ready",
            GameState.GameOver => "Game Over",
            _ => context.GameState.ToString(),
        };

        return $$"""
            BATTLESHIP GAME - YOUR ATTACK ANALYSIS
            ======================================

            GAME PROGRESS:
            - Board Size: {{context.BoardSize}}×{{context.BoardSize}}
            - Total Attacks Made: {{totalAttacks}} (Hit Ratio: {{hitRatio}})
            - Game State: {{gameStateDisplay}}

            ACCUMULATED ATTACK HISTORY (from your perspective):
            - HITS ({{context.Hits.Count}}): {{hitDisplay}}
            - MISSES ({{context.Misses.Count}}): {{missDisplay}}

            REMAINING OPPONENT SHIPS:
            - Carrier (5 spaces)
            - Battleship (4 spaces)
            - Cruiser (3 spaces)
            - Submarine (3 spaces)
            - Destroyer (2 spaces)

            AVAILABLE TARGETS:
            {{availableSample}}{{moreAvailable}}

            STRATEGIC ANALYSIS GUIDELINES:
            1. ADJACENCY STRATEGY: After hitting a ship, attack adjacent cells (up/down/left/right)
            2. PATTERN RECOGNITION: Look for rows/columns with multiple hits
            3. PROBABILITY MAPPING: Focus on untested areas near previous hits
            4. ELIMINATE IMPOSSIBLE: Ships cannot overlap, use this to narrow search
            5. SHIP SIZING: Remaining ship sizes help predict placement probability

            RESPOND WITH VALID JSON (no markdown formatting):
            {"cell": "A5", "reasoning": "Attacking adjacent to previous hit at A4"}

            Choose your next attack cell wisely:
            """;
    }

    /// <inheritdoc />
    public string BuildRetryPrompt(string originalPrompt, IReadOnlyList<string> availableTargets)
    {
        var cellList = string.Join(", ", availableTargets.Take(20));
        return $$"""
            {{originalPrompt}}

            IMPORTANT: You MUST respond with valid JSON containing "cell" and "reasoning" keys.
            Available cells include: {{cellList}}

            Example correct response:
            {"cell": "B5", "reasoning": "Targeting high-probability area"}

            Now respond with your cell choice:
            """;
    }
}
