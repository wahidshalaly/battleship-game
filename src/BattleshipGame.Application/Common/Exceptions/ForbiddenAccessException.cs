namespace BattleshipGame.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated caller is not allowed to perform the requested operation —
/// e.g. accessing a game they do not own, or acting before registering a player profile.
/// Surfaced as HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenAccessException(string message) : Exception(message);
