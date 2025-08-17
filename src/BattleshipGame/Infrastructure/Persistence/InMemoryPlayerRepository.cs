using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using System.Collections.Concurrent;

namespace BattleshipGame.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the Player repository.
/// </summary>
public class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly ConcurrentDictionary<PlayerId, Player> _players = new();

    /// <inheritdoc />
    public Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken cancellationToken)
    {
        _players.TryGetValue(playerId, out var player);

        return Task.FromResult(player);
    }

    /// <inheritdoc />
    public Task<PlayerId> SaveAsync(Player player, CancellationToken cancellationToken)
    {
        _players.AddOrUpdate(player.Id, player, (playerId, oldValue) => player);

        return Task.FromResult(player.Id);
    }

    /// <inheritdoc />
    public Task<Player?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var player = _players.Values.FirstOrDefault(
            p => string.Equals(p.Username, username, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(player);
    }

    /// <inheritdoc />
    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        var exists = _players.Values.Any(
            p => string.Equals(p.Username, username, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }
}
