namespace BattleshipGame.Application.Common.Exceptions;

/// <summary>
/// Thrown when username/password do not match. Maps to HTTP 401 Unauthorized.
/// </summary>
public sealed class InvalidCredentialsException() : Exception("Invalid username or password.");
