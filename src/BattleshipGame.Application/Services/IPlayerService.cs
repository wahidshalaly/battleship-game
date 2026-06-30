using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Services;

/// <summary>
/// Service interface for player-related operations.
/// </summary>
public interface IPlayerService
{
    Task<PlayerId> CreateAsync(string username, string identitySubject, CancellationToken ct);

    Task<GetPlayerQueryResult?> GetByIdAsync(PlayerId id, CancellationToken ct);

    Task<GetPlayerQueryResult?> GetByUsernameAsync(string username, CancellationToken ct);

    /// <summary>
    /// Returns the authenticated caller's <see cref="Player"/>, or <c>null</c> if they have no
    /// game profile yet.
    /// </summary>
    Task<Player?> GetCurrentAsync(CancellationToken ct);

    /// <summary>
    /// Returns the authenticated caller's <see cref="Player"/>, or throws
    /// <see cref="ForbiddenAccessException"/> (403) when they have no game profile.
    /// </summary>
    Task<Player> GetCurrentRequiredAsync(CancellationToken ct);
}
