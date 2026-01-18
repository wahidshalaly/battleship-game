using System.Text.Json;
using System.Text.RegularExpressions;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// A computer opponent uses Semantic Kernel and OpenAI-based LLM (Ollama or Azure OpenAI).
/// Provides explainable move selection with reasoning visible to players.
/// This is an Opponent, it only attacks the Player's board.
/// </summary>
public sealed class SemanticKernelOpponent(
    IPromptBuilder promptBuilder,
    Kernel kernel,
    ILogger<SemanticKernelOpponent> logger
) : IComputerOpponent
{
    private const int MaxRetries = 3;

    /// <inheritdoc />
    public OpponentStrategy Strategy => OpponentStrategy.SemanticKernel;

    /// <inheritdoc />
    public async Task<string> SelectNextAttackAsync(Game game, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Opponent: Analyzing game state for strategic move");

            var gameState = BuildGameStateContext(game);
            var availableTargets = gameState.AvailableTargets;

            if (availableTargets.Count == 0)
            {
                logger.LogWarning("Opponent: No available cells remaining");
                throw new InvalidOperationException("No available targets remaining.");
            }

            var prompt = promptBuilder.BuildStrategicPrompt(gameState);
            logger.LogDebug("Opponent: Prompt built, querying LLM");

            var selectedCell = await SelectCellWithRetryAsync(
                prompt,
                availableTargets,
                attemptNumber: 0,
                cancellationToken
            );

            if (availableTargets.Contains(selectedCell))
            {
                logger.LogInformation(
                    "Opponent: Selected cell {Cell} via strategic analysis",
                    selectedCell
                );
                return selectedCell;
            }

            logger.LogWarning(
                "Opponent: LLM selected invalid cell {Cell}, falling back to first available target",
                selectedCell
            );
            return availableTargets.First();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Opponent: Error during strategic selection, using fallback");
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
            logger.LogWarning("Opponent: Max retries reached, using first available cell");
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
                "Opponent: Failed to parse valid cell from LLM response (attempt {Attempt}), retrying",
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
                "Opponent: LLM call failed (attempt {Attempt}), retrying",
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
                        logger.LogInformation("Opponent: Parsed cell from JSON: {Cell}", cell);
                        return cell;
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogDebug(ex, "Opponent: Failed to parse JSON from response");
        }

        // Fallback: regex pattern matching for cell codes (e.g., A1, B10, J5)
        var matches = Regex.Matches(response, @"\b([A-J]\d{1,2})\b");
        foreach (Match match in matches)
        {
            var cell = match.Groups[1].Value.ToUpper();
            if (availableTargets.Contains(cell))
            {
                logger.LogInformation("Opponent: Parsed cell from regex: {Cell}", cell);
                return cell;
            }
        }

        logger.LogWarning(
            "Opponent: Could not parse any valid cell from response:\n{Response}",
            response
        );
        return null;
    }
}
