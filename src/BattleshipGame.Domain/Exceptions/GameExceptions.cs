using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Domain.Exceptions;

public class GameNotFoundException(Guid gameId) : Exception($"Game `{gameId}` is not found.");

public class GameNotReadyException(Guid gameId) : Exception($"Game `{gameId}` is not ready.");

public class InvalidGameStateException(Guid gameId, string expected, string actual)
    : Exception(
        $"Game `{gameId}` is in invalid state. Expected: `{expected}`, Actual: `{actual}`."
    );

public class GameNotStartedException(Guid gameId, GameState state)
    : Exception(
        $"Game `{gameId}` is not started. Cannot perform attacks. Current state: `{state}`"
    );

public class GameOverException(Guid gameId)
    : Exception($"Game `{gameId}` is over, no longer attacks are allowed.");

public class InvalidTargetSideException(Guid gameId, BoardSide current, BoardSide attacker)
    : Exception(
        $"It's not the player's turn. Game `{gameId}`, Current turn: `{current}`, Attacker turn: `{attacker}`."
    );
