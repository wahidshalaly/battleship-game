using System.Text.Json;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// A computer opponent that asks an LLM (through <see cref="ILlmChat"/>) for its next shot.
/// Builds the prompts and validates the reply; the model call itself sits behind the interface.
/// Provides explainable move selection with reasoning visible to players.
/// This is an Opponent, it only attacks the Player's board.
/// </summary>
public sealed class LlmOpponent(
    IPromptBuilder promptBuilder,
    ILlmChat llmChat,
    ILogger<LlmOpponent> logger
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

        var responseContent = await llmChat.CompleteAsync(systemPrompt, strategicPrompt, ct);
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
