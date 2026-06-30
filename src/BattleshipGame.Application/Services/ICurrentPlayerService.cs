using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Services;

/// <summary>
/// Resolves the <see cref="Player"/> profile that belongs to the authenticated caller.
/// </summary>
public interface ICurrentPlayerService
{
    /// <summary>
    /// Returns the current caller's player profile, or <c>null</c> if the caller is
    /// unauthenticated or has not registered a player yet.
    /// </summary>
    Task<Player?> GetAsync(CancellationToken ct);

    /// <summary>
    /// Returns the current caller's player profile, throwing
    /// <see cref="Common.Exceptions.ForbiddenAccessException"/> (403) when the caller is
    /// unauthenticated or has no registered player.
    /// </summary>
    Task<Player> GetRequiredAsync(CancellationToken ct);
}
