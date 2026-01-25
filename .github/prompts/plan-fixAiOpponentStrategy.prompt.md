## Plan: Fix AI Opponent Attack & Resilience Issues

Three critical bugs prevent graceful AI failure handling: the LLM repeatedly attacks already-hit cells because the prompt lacks an explicit valid targets list, game state corrupts when AI failures occur (turn stuck on opponent), and the resilience fallback doesn't trigger because it only catches `BrokenCircuitException` instead of `AiOpponentException`.

### Steps

1. **Update [ResilientComputerOpponentDecorator.cs](src/BattleshipGame.Infrastructure/Resilience/ResilientComputerOpponentDecorator.cs#L23-L41)** to catch `AiOpponentException` (not just `BrokenCircuitException`) in `SelectNextAttackAsync` and invoke `fallbackOpponent` when retries are exhausted.

2. **Wrap opponent attack in try-catch** in [GameplayService.cs](src/BattleshipGame.Application/Services/GameplayService.cs#L40-L76) `PlayerAttackThenCounterAttackAsync` method to handle `AiOpponentException` gracefully by creating a "miss" fallback result instead of propagating the exception.

3. **Modify [BattleshipPromptBuilder.cs](src/BattleshipGame.Infrastructure/ComputerOpponent/BattleshipPromptBuilder.cs#L30-L66)** `BuildStrategicPrompt` to include the explicit list of available cells from `GameSnapshot.AvailableTargets` instead of relying on "Any cell NOT in the attack history" instruction.

4. **Create unit tests** for `BattleshipPromptBuilder` (verify prompt contains all valid targets), `ResilientComputerOpponentDecorator` (verify fallback on `AiOpponentException`), and `GameplayService` (verify compensation logic).

5. **Enhance [GameApiSimulationTests.cs](tests/BattleshipGame.IntegrationTests/GameApiSimulationTests.cs)** to simulate AI failures by mocking `IComputerOpponent` to throw exceptions and verify game state remains consistent and fallback works correctly.

### Further Considerations

1. **Should we use `OpponentStrategy.Random` fallback or create a sentinel "skip turn" result?** The fix doc suggests creating a fallback `AttackResult.Miss` with `cellCode: "SKIPPED"`, but using `RandomAttackOpponent` is cleaner and maintains gameplay flow.

2. **Add observability?** Consider adding metrics for AI failure rate, fallback invocations, and circuit breaker state to track resilience effectiveness in production.

3. **Verify configuration mapping** - Found potential mismatch between `appsettings.json` using `SemanticKernelOptions` and code expecting `OpenAIOptions`. Should we validate configuration alignment?
