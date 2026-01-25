namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Exception thrown when an AI opponent fails to select a valid target cell.
/// This includes JSON parsing failures, validation errors, or business rule violations.
/// </summary>
public class AiOpponentException : Exception
{
    public AiOpponentException(string message)
        : base(message) { }

    public AiOpponentException(string message, Exception innerException)
        : base(message, innerException) { }
}
