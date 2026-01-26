namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Configuration options for OpenAI LLM integration.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// The model ID to use for chat completion (e.g., "gpt-4", "llama3.2", "phi3").
    /// </summary>
    public string? ModelId { get; set; } = null;

    /// <summary>
    /// The OpenAI-compatible API endpoint URL.
    /// </summary>
    public string? Endpoint { get; set; } = null;

    /// <summary>
    /// The API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; } = null;
}
