using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace BattleshipGame.Infrastructure.Resilience;

/// <summary>
/// Decorator that adds resilience (retry, circuit breaker, timeout) to any computer opponent.
/// When the circuit breaker opens, automatically falls back to a simpler strategy.
/// </summary>
public sealed class ResilientComputerOpponentDecorator(
    IComputerOpponent innerOpponent,
    ResiliencePipeline<string> resilientPipeline,
    IComputerOpponent fallbackOpponent,
    ILogger<ResilientComputerOpponentDecorator> logger
) : IComputerOpponent
{
    /// <inheritdoc />
    public OpponentStrategy Strategy => innerOpponent.Strategy;

    /// <inheritdoc />
    public async Task<string> SelectNextAttackAsync(Game game, CancellationToken ct)
    {
        try
        {
            return await resilientPipeline.ExecuteAsync(
                async ct2 => await innerOpponent.SelectNextAttackAsync(game, ct2),
                ct
            );
        }
        catch (BrokenCircuitException)
        {
            logger.LogWarning(
                "Circuit breaker is open for {Strategy} opponent due to repeated failures. Falling back to Random strategy.",
                innerOpponent.Strategy
            );

            // Use pre-resolved fallback opponent (avoids factory lookup during error handling)
            return await fallbackOpponent.SelectNextAttackAsync(game, ct);
        }
    }
}
