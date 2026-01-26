namespace BattleshipGame.Infrastructure.Resilience;

/// <summary>
/// Configuration options for OpenAI API resilience policies.
/// </summary>
public sealed class AiOpponentResilienceOptions
{
    public const string ConfigurationSectionName = "Resilience:AiOpponent";

    /// <summary>
    /// Maximum number of retry attempts before giving up.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Number of consecutive failures required to open the circuit breaker.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 3;

    /// <summary>
    /// Duration in seconds that the circuit breaker stays open after tripping.
    /// </summary>
    public int CircuitBreakerBreakDurationSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum time in seconds to wait for an API response before timing out.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Failure ratio threshold (0.0 to 1.0) for opening the circuit breaker.
    /// Default is 0.5 (50% failure rate).
    /// </summary>
    public double FailureRatio { get; set; } = 0.5;
}
