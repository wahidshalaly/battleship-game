namespace BattleshipGame.IntegrationTests;

/// <summary>
/// The API reads <c>OPENAI_MODEL_ID</c>, <c>OPENAI_ENDPOINT</c> and <c>OPENAI_API_KEY</c> from
/// environment variables at startup and refuses to start without them. Integration tests never
/// call a model (see <see cref="FakeChatCompletionService"/>), so these only need to satisfy that
/// startup check: placeholders are set for any that are missing in this test process.
/// </summary>
internal static class TestOpenAiSettings
{
    private static readonly (string Key, string Placeholder)[] Settings =
    [
        ("OPENAI_MODEL_ID", "test-model"),
        ("OPENAI_ENDPOINT", "http://127.0.0.1:9/v1"),
        ("OPENAI_API_KEY", "test-placeholder-not-a-real-key"),
    ];

    public static void EnsureEnvironment()
    {
        foreach (var (key, placeholder) in Settings)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, placeholder);
        }
    }
}
