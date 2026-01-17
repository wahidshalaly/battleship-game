using System.Text.Json;
using System.Text.RegularExpressions;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

/// <summary>
/// AI opponent strategy using Semantic Kernel and LLM (Ollama or Azure OpenAI).
/// Provides explainable move selection with reasoning visible to players.
/// </summary>
public class SemanticKernelStrategy(
    IGameRepository gameRepository,
    GameStateAnalyzer gameStateAnalyzer,
    Kernel kernel,
    ILogger<SemanticKernelStrategy> logger
) : IComputerOpponentStrategy
{
    private const int MaxRetries = 3;

    /// <summary>
    /// Selects the next attack cell using LLM-based strategic reasoning.
    /// Falls back to random cell if LLM fails or generates invalid move.
    /// </summary>
    public async Task<string> SelectNextAttack(GameId gameId)
    {
        try
        {
            logger.LogInformation("AI: Analyzing game state for strategic move");

            var gameState = await gameStateAnalyzer.AnalyzeGameStateAsync(
                gameId,
                CancellationToken.None
            );

            var nextTargets = gameState.NextTargets;
            if (!nextTargets.Any())
            {
                logger.LogWarning("AI: No available cells remaining");
                return nextTargets.First();
            }

            // Build prompt with accumulated game context
            var prompt = BuildStrategicPrompt(gameState);
            logger.LogDebug("AI: Prompt built, querying LLM");

            // Query LLM with retry logic
            string selectedCell = await SelectCellWithRetry(prompt, nextTargets, 0);

            // Validate the selected cell
            if (nextTargets.Contains(selectedCell))
            {
                logger.LogInformation(
                    "AI: Selected cell {Cell} via strategic analysis",
                    selectedCell
                );
                return selectedCell;
            }

            logger.LogWarning(
                "AI: LLM selected invalid cell {Cell}, falling back to first random target",
                selectedCell
            );
            return nextTargets.First();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI: Error during strategic selection, using fallback");
            // Fallback: return first available cell
            var game = await gameRepository.GetByIdAsync(gameId, CancellationToken.None);
            var cells = game?.GetNextTargets(BoardSide.Player).ToList() ?? new List<string>();
            return cells.FirstOrDefault() ?? "A1";
        }
    }

    /// <summary>
    /// Attempts to get a valid cell selection from LLM with retry logic.
    /// </summary>
    private async Task<string> SelectCellWithRetry(
        string prompt,
        List<string> nextTargets,
        int attemptNumber
    )
    {
        if (attemptNumber >= MaxRetries)
        {
            logger.LogWarning("AI: Max retries reached, using first available cell");
            return nextTargets.First();
        }

        try
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var messages = new ChatHistory();
            messages.AddSystemMessage(GetSystemPrompt());
            messages.AddUserMessage(prompt);

            var response = await chatService.GetChatMessageContentAsync(messages, kernel: kernel);

            var selectedCell = ParseCellFromResponse(response.Content ?? string.Empty, nextTargets);

            if (selectedCell != null)
            {
                return selectedCell;
            }

            logger.LogWarning(
                "AI: Failed to parse valid cell from LLM response (attempt {Attempt}), retrying",
                attemptNumber + 1
            );

            // Retry with slightly different prompt
            var retryPrompt = BuildRetryPrompt(prompt, nextTargets);
            return await SelectCellWithRetry(retryPrompt, nextTargets, attemptNumber + 1);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "AI: LLM call failed (attempt {Attempt}), retrying",
                attemptNumber + 1
            );

            // Retry
            return await SelectCellWithRetry(prompt, nextTargets, attemptNumber + 1);
        }
    }

    /// <summary>
    /// Extracts a valid cell code from LLM response.
    /// Tries JSON parsing first, then regex pattern matching.
    /// </summary>
    private string? ParseCellFromResponse(string response, List<string> nextTargets)
    {
        // Try JSON parsing first
        try
        {
            var jsonMatch = Regex.Match(response, @"\{[^}]*""cell""[^}]*\}");
            if (jsonMatch.Success)
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonMatch.Value);
                if (json?.TryGetValue("cell", out var cellObj) == true)
                {
                    var cell = cellObj?.ToString()?.Trim().ToUpper();
                    if (!string.IsNullOrEmpty(cell) && nextTargets.Contains(cell))
                    {
                        logger.LogInformation("AI: Parsed cell from JSON: {Cell}", cell);
                        return cell;
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogDebug(ex, "AI: Failed to parse JSON from response");
        }

        // Fallback: regex pattern matching for cell codes (e.g., A1, B10, J5)
        var matches = Regex.Matches(response, @"\b([A-J]\d{1,2})\b");
        foreach (Match match in matches)
        {
            var cell = match.Groups[1].Value.ToUpper();
            if (nextTargets.Contains(cell))
            {
                logger.LogInformation("AI: Parsed cell from regex: {Cell}", cell);
                return cell;
            }
        }

        logger.LogWarning(
            "AI: Could not parse any valid cell from response:\n{Response}",
            response
        );
        return null;
    }

    /// <summary>
    /// Builds the strategic prompt with accumulated game history and context.
    /// Shows the AI's perspective on hits, misses, and patterns discovered so far.
    /// </summary>
    private static string BuildStrategicPrompt(GameStateContext gameState)
    {
        // Format hit cells for display
        var hitDisplay = gameState.Hits.Any()
            ? string.Join(", ", gameState.Hits.OrderBy(c => c))
            : "None yet";

        // Format missed cells for display
        var missedDisplay = gameState.Misseds.Any()
            ? string.Join(", ", gameState.Misseds.OrderBy(c => c))
            : "None yet";

        // Show recent hits for pattern detection
        var recentHitsDisplay = gameState.RecentHits.Any()
            ? string.Join(", ", gameState.RecentHits.OrderBy(c => c))
            : "No pattern detected";

        // Calculate hit/miss ratio for confidence
        var totalAttacks = gameState.Hits.Count + gameState.Misseds.Count;
        var hitRatio = totalAttacks > 0 ? $"{(gameState.Hits.Count * 100 / totalAttacks)}%" : "0%";

        // Sample available cells
        var availableSample = string.Join(", ", gameState.NextTargets.Take(15));
        var moreAvailable =
            gameState.NextTargets.Count > 15
                ? $" ...and {gameState.NextTargets.Count - 15} more cells"
                : "";

        return $$"""
BATTLESHIP GAME - YOUR ATTACK ANALYSIS
======================================

GAME PROGRESS:
- Board Size: {{gameState.BoardSize}}×{{gameState.BoardSize}}
- Total Attacks Made: {{totalAttacks}} (Hit Ratio: {{hitRatio}})
- Ships Destroyed: {{gameState.ShipsSunk}}/5
- Game Phase: {{gameState.GamePhase}}

ACCUMULATED ATTACK HISTORY (from your perspective):
- HITS ({{gameState.Hits.Count}}): {{hitDisplay}}
- MISSES ({{gameState.Misseds.Count}}): {{missedDisplay}}
- RECENT HITS PATTERN: {{recentHitsDisplay}}

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

    /// <summary>
    /// Builds a retry prompt with additional guidance.
    /// </summary>
    private static string BuildRetryPrompt(string originalPrompt, List<string> nextTargets)
    {
        var cellList = string.Join(", ", nextTargets.Take(20));
        return $$"""
{{originalPrompt}}

IMPORTANT: You MUST respond with valid JSON containing "cell" and "reasoning" keys.
Available cells include: {{cellList}}

Example correct response:
{"cell": "B5", "reasoning": "Targeting high-probability area"}

Now respond with your cell choice:
""";
    }

    /// <summary>
    /// System prompt that defines the AI's role and constraints.
    /// </summary>
    private static string GetSystemPrompt()
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
}
