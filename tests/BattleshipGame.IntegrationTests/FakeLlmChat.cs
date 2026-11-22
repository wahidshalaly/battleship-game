using System.Collections.Concurrent;
using BattleshipGame.Infrastructure.ComputerOpponent;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// Scripted stand-in for the LLM behind the LLM opponent. Integration tests never call a real
/// model: everything from the HTTP request down to <c>LlmOpponent</c> (prompt building, reply
/// parsing, the resilience decorator and its fallback) runs for real, and only
/// <see cref="ILlmChat"/> is replaced.
/// </summary>
/// <remarks>
/// A call with no scripted reply fails, but the resilience decorator turns any failure into a
/// Random fallback, so the HTTP request still succeeds. Such calls are therefore recorded, and
/// tests call <see cref="VerifyAllCallsScripted"/> to fail on them.
/// </remarks>
internal sealed class FakeLlmChat : ILlmChat
{
    // Set by the test thread, read by the server's request thread.
    private volatile Func<string> _reply;
    private int _callCount;
    private int _unscriptedCallCount;

    public FakeLlmChat() => _reply = () => FailUnscripted("no reply scripted for this test");

    /// <summary>Number of model calls received, including ones that failed.</summary>
    public int CallCount => Volatile.Read(ref _callCount);

    /// <summary>Answers each call with the next cell, as the JSON the opponent expects.</summary>
    public void ReplyWithCells(IEnumerable<string> cells)
    {
        var queue = new ConcurrentQueue<string>(cells);
        _reply = () =>
            queue.TryDequeue(out var cell)
                ? $$"""{"cell":"{{cell}}","reasoning":"scripted"}"""
                : FailUnscripted("scripted cells exhausted");
    }

    /// <summary>Answers every call with the given raw content.</summary>
    public void ReplyRaw(string content) => _reply = () => content;

    /// <summary>Fails every call with the given exception.</summary>
    public void Throw(Exception exception) => _reply = () => throw exception;

    /// <summary>
    /// Registers this fake as the <see cref="ILlmChat"/>. Registered after the API's own
    /// services, so it replaces the Semantic Kernel-backed one.
    /// </summary>
    public void Register(IServiceCollection services) => services.AddSingleton<ILlmChat>(this);

    /// <summary>Fails the test if any call found no scripted reply.</summary>
    public void VerifyAllCallsScripted() =>
        Volatile
            .Read(ref _unscriptedCallCount)
            .Should()
            .Be(0, "every model call in this test should have a scripted reply");

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return Task.FromCanceled<string>(ct);

        Interlocked.Increment(ref _callCount);
        try
        {
            return Task.FromResult(_reply());
        }
        catch (Exception ex)
        {
            // Fail through the returned task, as a real (async) client does.
            return Task.FromException<string>(ex);
        }
    }

    private string FailUnscripted(string reason)
    {
        Interlocked.Increment(ref _unscriptedCallCount);
        throw new InvalidOperationException($"{nameof(FakeLlmChat)}: {reason}.");
    }
}
