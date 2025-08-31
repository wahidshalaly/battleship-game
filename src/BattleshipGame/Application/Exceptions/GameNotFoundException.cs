namespace BattleshipGame.Application.Exceptions;

public class GameNotFoundException(Guid gameId) : Exception($"Game with ID {gameId} not found.") { }
