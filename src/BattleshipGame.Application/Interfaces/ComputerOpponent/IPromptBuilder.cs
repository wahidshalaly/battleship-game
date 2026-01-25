using BattleshipGame.Application.Common;

namespace BattleshipGame.Application.Interfaces.ComputerOpponent;

/// <summary>
/// Builds prompts for LLM-based opponent strategies.
/// </summary>
public interface IPromptBuilder
{
    /// <summary>
    /// Builds the system prompt that defines the AI's role and constraints.
    /// </summary>
    /// <returns>The system prompt string.</returns>
    string BuildSystemPrompt();

    /// <summary>
    /// Builds the strategic prompt with game context for attack selection.
    /// </summary>
    /// <param name="context">The current game state context.</param>
    /// <returns>The strategic prompt string.</returns>
    string BuildStrategicPrompt(GameSnapshot context);
}
