using System.Collections.Concurrent;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the Game repository.
/// </summary>
public class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<GameId, Game> _games = new();

    /// <inheritdoc />
    public Task<Game?> GetByIdAsync(GameId gameId, CancellationToken cancellationToken)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

    /// <inheritdoc />
    public Task SaveAsync(Game game, CancellationToken cancellationToken)
    {
        _games.AddOrUpdate(game.Id, game, (_, _) => game);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Game>> GetByPlayerIdAsync(
        PlayerId playerId,
        CancellationToken cancellationToken
    )
    {
        var playerGames = _games.Values.Where(g => g.PlayerId == playerId).ToList().AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<Game>>(playerGames);
    }

    /// <inheritdoc />
    public Task<Game?> GetActiveGameByPlayerIdAsync(
        PlayerId playerId,
        CancellationToken cancellationToken
    )
    {
        var game = _games.Values.LastOrDefault(g =>
            g.PlayerId == playerId && g.State != GameState.GameOver
        );

        return Task.FromResult(game);
    }
}
