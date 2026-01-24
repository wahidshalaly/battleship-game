## Plan: Circuit Breaker for OpenAI API Calls

Use information from https://github.com/App-vNext/Polly/blob/main/README.md to implement retry and circuit breaker patterns for handling OpenAI API rate limits (HTTP 429).

The circuit breaker should handle rate limits. When triggered, the system will temporarily stop calling the API and fallback to `RandomAttackStrategy` for a defined period. The implementation should mainly use Polly and Microsoft extensions for implementation based on the documentation above.

### Steps
1. Identify where OpenAI API calls are made in [BattleshipGame.Application/Interfaces/ComputerOpponent](src/BattleshipGame.Application/Interfaces/ComputerOpponent) and [BattleshipGame.Infrastructure/ComputerOpponent](src/BattleshipGame.Infrastructure/ComputerOpponent).
2. Update the OpenAI API call logic to:
   - Check circuit state before calling.
   - On HTTP 429, open the circuit for a configurable duration.
   - While open, use `RandomAttackStrategy` as fallback.
3. Register the `CircuitBreaker` in DI and inject where needed.
4. Add configuration for circuit breaker timing in [appsettings.json](src/BattleshipGame.AppHost/appsettings.json).
5. Update or add tests in [BattleshipGame.UnitTests](tests/BattleshipGame.UnitTests) to cover circuit breaker and fallback logic.

### Further Considerations
1. What fallback duration is appropriate? (e.g., 1 min, 5 min, configurable)
It should be exponentially backed off on repeated failures. Use default values and sensible limits.
2. Should circuit breaker state be per-user, per-game, or global?
It should be global to prevent overwhelming the API.
3. Consider logging circuit breaker events for monitoring and diagnostics.
4. Ensure thread-safety in the `CircuitBreaker` implementation if the application is multi-threaded.
5. Review existing retry policies to ensure they align with the circuit breaker behavior.
