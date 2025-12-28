namespace BattleshipGame.Application.Exceptions;

public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException(Guid playerId)
        : base($"Player `{playerId}` is not found.") { }

    public PlayerNotFoundException(string username)
        : base($"Player `{username}` is not found.") { }
}

public class PlayerIsInActiveException(Guid playerId)
    : Exception($"Player `{playerId}` is in an active game.");

public class PlayerIsNotInActiveException(Guid playerId)
    : Exception($"Player `{playerId}` is NOT in an active game.");
