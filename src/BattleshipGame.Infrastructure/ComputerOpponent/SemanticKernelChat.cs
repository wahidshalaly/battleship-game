using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// <see cref="ILlmChat"/> backed by a Semantic Kernel chat completion service (any
/// OpenAI-compatible endpoint, e.g. Ollama or Azure OpenAI). The only place that uses the
/// Semantic Kernel API.
/// </summary>
public sealed class SemanticKernelChat(IChatCompletionService chatCompletion) : ILlmChat
{
    /// <inheritdoc />
    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct
    )
    {
        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);
        history.AddUserMessage(userPrompt);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 100, // The opponent expects a short JSON reply
            Temperature = 0.3, // More deterministic
        };

        var response = await chatCompletion.GetChatMessageContentAsync(
            history,
            executionSettings,
            cancellationToken: ct
        );
        return response.Content ?? string.Empty;
    }
}
