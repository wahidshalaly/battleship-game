using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using System.Collections.Concurrent;

namespace BattleshipGame.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the Game repository.
/// </summary>
public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<GameId, Game> _games = new();

    /// <inheritdoc />
    public Task<Game?> GetByIdAsync(GameId gameId, CancellationToken ct = default)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

    /// <inheritdoc />
    public Task<GameId> SaveAsync(Game game, CancellationToken ct = default)
    {
        _games.AddOrUpdate(game.Id, game, (key, oldValue) => game);
        return Task.FromResult(game.Id);
    }

    /// <inheritdoc />
    public Task DeleteAsync(GameId gameId, CancellationToken ct = default)
    {
        _games.TryRemove(gameId, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Game>> GetByPlayerIdAsync(PlayerId playerId, CancellationToken ct = default)
    {
        var playerGames = _games.Values
            .Where(game => game.PlayerId == playerId)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<Game>>(playerGames);
    }

    /// <inheritdoc />
    public Task<Game?> GetActiveGameByPlayerIdAsync(PlayerId playerId, CancellationToken ct = default)
    {
        var activeGame = _games.Values
            .FirstOrDefault(game =>
                game.PlayerId == playerId &&
                game.State != Domain.DomainModel.Common.GameState.GameOver);

        return Task.FromResult(activeGame);
    }
}
