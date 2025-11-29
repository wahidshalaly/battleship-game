namespace BattleshipGame.Application.Exceptions;

public class GameNotFoundException(Guid gameId) : Exception($"Game `{gameId}` is not found.");
