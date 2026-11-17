namespace BattleshipGame.IntegrationTests;

/// <summary>
/// The API reads <c>OPENAI_MODEL_ID</c>, <c>OPENAI_ENDPOINT</c> and <c>OPENAI_API_KEY</c> from
/// environment variables at startup and refuses to start without them. Sets placeholders for any
/// that are missing in this test process, so integration tests run without a configured LLM.
/// The placeholder endpoint is unreachable, which exercises the resilience fallback to the Random
/// strategy. Values already set (CI placeholders, or a real local model) are left untouched.
/// </summary>
internal static class TestOpenAiSettings
{
    private static readonly (string Key, string Placeholder)[] Settings =
    [
        ("OPENAI_MODEL_ID", "test-model"),
        ("OPENAI_ENDPOINT", "http://127.0.0.1:9/v1"),
        ("OPENAI_API_KEY", "test-placeholder-not-a-real-key"),
    ];

    /// <summary>
    /// Whether an API key was configured before any placeholders were applied, i.e. whether a
    /// live model might be reachable. Captured on first use of this type.
    /// </summary>
    public static bool HasConfiguredApiKey { get; } =
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

    public static void EnsureEnvironment()
    {
        foreach (var (key, placeholder) in Settings)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, placeholder);
        }
    }
}
