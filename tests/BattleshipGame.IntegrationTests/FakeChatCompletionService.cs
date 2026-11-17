using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// Scripted stand-in for the LLM behind the SemanticKernel opponent. Integration tests never
/// call a real model: everything from the HTTP request down to <c>SemanticKernelOpponent</c>
/// (prompt building, reply parsing, the resilience decorator and its fallback) runs for real,
/// and only the chat completion is replaced. Unscripted calls fail loudly.
/// </summary>
internal sealed class FakeChatCompletionService : IChatCompletionService
{
    private Func<string> _reply = () =>
        throw new InvalidOperationException(
            $"{nameof(FakeChatCompletionService)}: no reply scripted for this test."
        );
    private int _callCount;

    /// <summary>Number of chat completion requests received.</summary>
    public int CallCount => Volatile.Read(ref _callCount);

    public IReadOnlyDictionary<string, object?> Attributes { get; } =
        new Dictionary<string, object?>();

    /// <summary>Answers each request with the next cell, as the JSON the opponent expects.</summary>
    public void ReplyWithCells(IEnumerable<string> cells)
    {
        var queue = new Queue<string>(cells);
        _reply = () =>
        {
            lock (queue)
            {
                return queue.TryDequeue(out var cell)
                    ? $$"""{"cell":"{{cell}}","reasoning":"scripted"}"""
                    : throw new InvalidOperationException(
                        $"{nameof(FakeChatCompletionService)}: scripted cells exhausted."
                    );
            }
        };
    }

    /// <summary>Answers every request with the given raw content.</summary>
    public void ReplyRaw(string content) => _reply = () => content;

    /// <summary>Fails every request with the given exception.</summary>
    public void Throw(Exception exception) => _reply = () => throw exception;

    /// <summary>
    /// Registers a <see cref="Kernel"/> backed by this fake. Registered after the API's own
    /// services, so it replaces the OpenAI-backed kernel.
    /// </summary>
    public void Register(IServiceCollection services)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton<IChatCompletionService>(this);
        services.AddSingleton(kernelBuilder.Build());
    }

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default
    )
    {
        Interlocked.Increment(ref _callCount);
        IReadOnlyList<ChatMessageContent> result =
        [
            new ChatMessageContent(AuthorRole.Assistant, _reply()),
        ];
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException("The opponent does not stream.");
}
