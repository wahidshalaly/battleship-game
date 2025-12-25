using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Services;

/// <summary>
/// Service interface for player-related operations.
/// </summary>
public interface IPlayerService
{
    /// <summary>
    /// Creates a new player with the specified username.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<PlayerId> CreateAsync(string username, CancellationToken ct);

    /// <summary>
    /// Gets a player by their unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<GetPlayerQueryResult?> GetByIdAsync(PlayerId id, CancellationToken ct);
    Task<GetPlayerQueryResult?> GetByUsernameAsync(string username, CancellationToken ct);
}
