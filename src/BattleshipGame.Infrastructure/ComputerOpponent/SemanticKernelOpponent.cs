using System.Text.Json;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc />
    public OpponentStrategy Strategy => OpponentStrategy.SemanticKernel;

    /// <inheritdoc />
    public async Task<string> SelectNextAttackAsync(Game game, CancellationToken ct)
    {
        logger.LogInformation("Opponent: Analyzing game state for strategic move");

        var gameSnapshot = BuildGameSnapshot(game);
        var systemPrompt = promptBuilder.BuildSystemPrompt();
        var strategicPrompt = promptBuilder.BuildStrategicPrompt(gameSnapshot);
        logger.LogDebug("Opponent: Prompt built. Strategic Prompt: {Prompt}", strategicPrompt);

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var messages = new ChatHistory();
        messages.AddSystemMessage(systemPrompt);
        messages.AddUserMessage(strategicPrompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 100, // Short JSON response only
            Temperature = 0.3, // More deterministic
        };

        var response = await chatService.GetChatMessageContentAsync(
            messages,
            executionSettings,
            kernel,
            ct
        );

        var responseContent = response.Content ?? string.Empty;
        logger.LogDebug("Opponent: LLM response: {Response}", responseContent);
        var availableTargets = gameSnapshot.AvailableTargets;
        var selectedCell = ParseCellFromResponse(responseContent, availableTargets);

        logger.LogInformation(
            "Opponent: Selected cell {Cell} via strategic analysis",
            selectedCell
        );
        return selectedCell;
    }

    /// <summary>
    /// Builds a read-only game state context for prompt construction.
    /// </summary>
    private static GameSnapshot BuildGameSnapshot(Game game)
    {
        return new GameSnapshot
        {
            BoardSize = game.BoardSize,
            GameState = game.State,
            BoardDescription = game.DescribeBoard(),
            AvailableTargets = [.. game.GetNextTargets(BoardSide.Player)],
            Hits = [.. game.GetHits(BoardSide.Player)],
            Misses = [.. game.GetMisseds(BoardSide.Player)],
        };
    }

    private string ParseCellFromResponse(string response, IReadOnlyList<string> availableTargets)
    {
        AiOpponentNextTarget aiOpponentNextTarget;

        try
        {
            aiOpponentNextTarget =
                JsonSerializer.Deserialize<AiOpponentNextTarget>(response, jsonSerializerOptions)
                ?? throw new AiOpponentException("Deserialized response is null");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Opponent: Failed to deserialize JSON response");
            throw new AiOpponentException("Failed to parse LLM response as JSON", ex);
        }

        var cell = aiOpponentNextTarget.Cell?.Trim().ToUpper();

        if (string.IsNullOrEmpty(cell))
        {
            logger.LogWarning("Opponent: Cell property is null or empty in response");
            throw new AiOpponentException("LLM response contains null or empty cell value");
        }

        if (!availableTargets.Contains(cell))
        {
            logger.LogWarning(
                "Opponent: LLM selected unavailable cell {Cell}. Available cells: {AvailableCount}",
                cell,
                availableTargets.Count
            );
            throw new AiOpponentException($"LLM selected unavailable cell: {cell}");
        }

        logger.LogInformation(
            "Opponent: Successfully parsed cell {Cell} (Reasoning: {Reasoning})",
            cell,
            aiOpponentNextTarget.Reasoning
        );
        return cell;
    }
}
