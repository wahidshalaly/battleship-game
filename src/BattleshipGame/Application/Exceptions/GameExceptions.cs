using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Exceptions;

public class GameNotFoundException(Guid gameId) : Exception($"Game `{gameId}` is not found.");

public class GameNotReadyException(Guid gameId) : Exception($"Game `{gameId}` is not ready.");

public class InvalidGameStateException(Guid gameId, string expected, string actual)
    : Exception(
        $"Game `{gameId}` is in invalid state. Expected: `{expected}`, Actual: `{actual}`."
    );

public class NotPlayerTurnException(Guid gameId, BoardSide current, BoardSide attacker)
    : Exception(
        $"It's not the player's turn. Game `{gameId}`, Current turn: `{current}`, Attacker turn: `{attacker}`."
    );
