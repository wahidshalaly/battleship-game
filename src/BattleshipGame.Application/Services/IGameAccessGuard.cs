using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Services;

/// <summary>
/// Enforces that the authenticated caller may only access games they own.
/// </summary>
public interface IGameAccessGuard
{
    /// <summary>
    /// Ensures the current caller owns the specified game.
    /// </summary>
    /// <exception cref="Domain.Exceptions.GameNotFoundException">The game does not exist (404).</exception>
    /// <exception cref="Common.Exceptions.ForbiddenAccessException">
    /// The caller is not the owner, is unauthenticated, or has no player profile (403).
    /// </exception>
    Task EnsureOwnerAsync(GameId gameId, CancellationToken ct);
}
