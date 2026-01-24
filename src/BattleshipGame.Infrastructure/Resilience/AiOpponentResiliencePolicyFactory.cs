using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace BattleshipGame.Infrastructure.Resilience;

/// <summary>
/// Factory for creating resilience policies for OpenAI API calls.
/// Handles rate limiting (HTTP 429) with exponential backoff retry and circuit breaker pattern.
/// Uses Polly v8 API with ResiliencePipeline.
/// </summary>
public static class AiOpponentResiliencePolicyFactory
{
    /// <summary>
    /// Creates a combined resilience pipeline that includes retry, circuit breaker, and timeout strategies.
    /// Order: Timeout -> CircuitBreaker -> Retry
    /// </summary>
    public static ResiliencePipeline<T> CreateCombinedPipeline<T>(
        AiOpponentResilienceOptions options,
        ILogger logger
    )
    {
        var builder = new ResiliencePipelineBuilder<T>();

        // Add timeout strategy
        builder.AddTimeout(TimeSpan.FromSeconds(options.TimeoutSeconds));

        // Add retry strategy with exponential backoff
        builder.AddRetry(
            new RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>(ex =>
                    {
                        // Check if it's a 429 Too Many Requests error
                        var isRateLimited =
                            ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests;
                        if (isRateLimited)
                        {
                            logger.LogWarning(
                                "OpenAI API rate limit (HTTP 429) detected. Retrying with exponential backoff."
                            );
                        }
                        return isRateLimited;
                    })
                    .Handle<OperationCanceledException>(),
                MaxRetryAttempts = options.MaxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                UseJitter = true,
            }
        );

        // Add circuit breaker strategy
        builder.AddCircuitBreaker(
            new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = options.FailureRatio,
                MinimumThroughput = options.CircuitBreakerThreshold,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds),
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<HttpRequestException>()
                    .Handle<OperationCanceledException>(),
            }
        );

        return builder.Build();
    }
}
