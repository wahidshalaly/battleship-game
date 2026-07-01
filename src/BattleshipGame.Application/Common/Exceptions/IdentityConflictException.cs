namespace BattleshipGame.Application.Common.Exceptions;

/// <summary>
/// Thrown when a registration attempt conflicts with an existing identity (duplicate
/// username or email). Maps to HTTP 409 Conflict.
/// </summary>
public sealed class IdentityConflictException(string message) : Exception(message);
