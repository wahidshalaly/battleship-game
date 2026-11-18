namespace BattleshipGame.Domain.Exceptions;

public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException(Guid playerId)
        : base($"Player `{playerId}` is not found.") { }

    public PlayerNotFoundException(string username)
        : base($"Player `{username}` is not found.") { }
}

public class PlayerHasActiveGameException(Guid playerId, Guid activeGameId)
    : Exception($"Player `{playerId}` already has an active game `{activeGameId}`.")
{
    public Guid ActiveGameId { get; } = activeGameId;
}
