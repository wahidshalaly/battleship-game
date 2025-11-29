namespace BattleshipGame.Application.Exceptions;

public class PlayerNotFoundException(Guid playerId)
    : Exception($"Player `{playerId}` is not found.");

public class PlayerIsInActiveException(Guid playerId)
    : Exception($"Player `{playerId}` is in an active game.");

public class PlayerIsNotInActiveException(Guid playerId)
    : Exception($"Player `{playerId}` is NOT in an active game.");
