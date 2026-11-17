namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// The single LLM capability the <see cref="LlmOpponent"/> needs: send a system and a user
/// prompt, get the model's text reply back. Owned by this project so the opponent and its tests
/// do not depend on an AI SDK; <see cref="SemanticKernelChat"/> is the SDK-backed implementation.
/// </summary>
public interface ILlmChat
{
    /// <summary>Returns the model's reply to the given prompts (empty if it sent no text).</summary>
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct);
}
