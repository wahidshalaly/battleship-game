using System.Text.Json;
using System.Text.RegularExpressions;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BattleshipGame.Infrastructure.OpponentStrategy;

/// <summary>
/// AI opponent strategy using Semantic Kernel and LLM (Ollama or Azure OpenAI).
/// Provides explainable move selection with reasoning visible to players.
/// Always attacks the Player's board.
/// </summary>
public sealed class SemanticKernelStrategy(
    IPromptBuilder promptBuilder,
    Kernel kernel,
    ILogger<SemanticKernelStrategy> logger
) : IComputerOpponentStrategy
{
    private const int MaxRetries = 3;

    /// <inheritdoc />
    public OpponentStrategyType StrategyType => OpponentStrategyType.SemanticKernel;

    /// <inheritdoc />
    public async Task<string> SelectNextAttackAsync(Game game, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("AI: Analyzing game state for strategic move");

            var gameState = BuildGameStateContext(game);
            var availableTargets = gameState.AvailableTargets;

            if (availableTargets.Count == 0)
            {
                logger.LogWarning("AI: No available cells remaining");
                throw new InvalidOperationException("No available targets remaining.");
            }

            var prompt = promptBuilder.BuildStrategicPrompt(gameState);
            logger.LogDebug("AI: Prompt built, querying LLM");

            var selectedCell = await SelectCellWithRetryAsync(
                prompt,
                availableTargets,
                attemptNumber: 0,
                cancellationToken
            );

            if (availableTargets.Contains(selectedCell))
            {
                logger.LogInformation(
                    "AI: Selected cell {Cell} via strategic analysis",
                    selectedCell
                );
                return selectedCell;
            }

            logger.LogWarning(
                "AI: LLM selected invalid cell {Cell}, falling back to first available target",
                selectedCell
            );
            return availableTargets.First();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "AI: Error during strategic selection, using fallback");
            var availableTargets = game.GetNextTargets(BoardSide.Player);
            return availableTargets.FirstOrDefault() ?? "A1";
        }
    }

    /// <summary>
    /// Builds a read-only game state context for prompt construction.
    /// </summary>
    private static GameStateContext BuildGameStateContext(Game game)
    {
        return new GameStateContext
        {
            BoardSize = game.BoardSize,
            GameState = game.State,
            AvailableTargets = game.GetNextTargets(BoardSide.Player).ToList(),
            Hits = game.GetHits(BoardSide.Player).ToList(),
            Misses = game.GetMisseds(BoardSide.Player).ToList(),
        };
    }

    private async Task<string> SelectCellWithRetryAsync(
        string prompt,
        IReadOnlyList<string> availableTargets,
        int attemptNumber,
        CancellationToken cancellationToken
    )
    {
        if (attemptNumber >= MaxRetries)
        {
            logger.LogWarning("AI: Max retries reached, using first available cell");
            return availableTargets.First();
        }

        try
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var messages = new ChatHistory();
            messages.AddSystemMessage(promptBuilder.BuildSystemPrompt());
            messages.AddUserMessage(prompt);

            var response = await chatService.GetChatMessageContentAsync(
                messages,
                kernel: kernel,
                cancellationToken: cancellationToken
            );

            var selectedCell = ParseCellFromResponse(
                response.Content ?? string.Empty,
                availableTargets
            );

            if (selectedCell is not null)
            {
                return selectedCell;
            }

            logger.LogWarning(
                "AI: Failed to parse valid cell from LLM response (attempt {Attempt}), retrying",
                attemptNumber + 1
            );

            var retryPrompt = promptBuilder.BuildRetryPrompt(prompt, availableTargets);
            return await SelectCellWithRetryAsync(
                retryPrompt,
                availableTargets,
                attemptNumber + 1,
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "AI: LLM call failed (attempt {Attempt}), retrying",
                attemptNumber + 1
            );

            return await SelectCellWithRetryAsync(
                prompt,
                availableTargets,
                attemptNumber + 1,
                cancellationToken
            );
        }
    }

    private string? ParseCellFromResponse(string response, IReadOnlyList<string> availableTargets)
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
                    if (!string.IsNullOrEmpty(cell) && availableTargets.Contains(cell))
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
            if (availableTargets.Contains(cell))
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
}
