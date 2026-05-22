using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.Infrastructure.Persistence.Repositories;

internal class PlayerRepository(BattleshipGameDbContext context) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken ct)
    {
        var entity = await context
            .Players.Include(p => p.GameHistory)
            .FirstOrDefaultAsync(p => p.Id == playerId.Value, ct);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<Player> GetByIdOrThrowAsync(PlayerId playerId, CancellationToken ct)
    {
        var entity =
            await context
                .Players.Include(p => p.GameHistory)
                .FirstOrDefaultAsync(p => p.Id == playerId.Value, ct)
            ?? throw new Domain.Exceptions.PlayerNotFoundException(playerId);

        return MapToDomain(entity);
    }

    public async Task<PlayerId> SaveAsync(Player player, CancellationToken ct)
    {
        var existing = await context
            .Players.Include(p => p.GameHistory)
            .FirstOrDefaultAsync(p => p.Id == player.Id.Value, ct);

        if (existing is null)
        {
            var entity = MapToEntity(player);
            await context.Players.AddAsync(entity, ct);
        }
        else
        {
            MapToExistingEntity(player, existing);
        }

        return player.Id;
    }

    public async Task<Player?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        var entity = await context
            .Players.Include(p => p.GameHistory)
            .FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower(), ct);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct) =>
        await context.Players.AnyAsync(p => p.Username.ToLower() == username.ToLower(), ct);

    private static Player MapToDomain(PlayerEntity entity)
    {
        var playerId = new PlayerId(entity.Id);
        var activeGameId = entity.ActiveGameId.HasValue
            ? new GameId(entity.ActiveGameId.Value)
            : null;

        var player = new Player(playerId, entity.Username, activeGameId);
        player.RestoreGameHistory(entity.GameHistory.Select(h => new GameId(h.GameId)));
        return player;
    }

    private static PlayerEntity MapToEntity(Player player) =>
        new()
        {
            Id = player.Id.Value,
            Username = player.Username,
            ActiveGameId = player.ActiveGameId?.Value,
            GameHistory = player
                .GameHistory.Select(gid => new PlayerGameHistoryEntry
                {
                    PlayerId = player.Id.Value,
                    GameId = gid.Value,
                })
                .ToList(),
        };

    private static void MapToExistingEntity(Player player, PlayerEntity entity)
    {
        entity.ActiveGameId = player.ActiveGameId?.Value;

        var existingGameIds = entity.GameHistory.Select(h => h.GameId).ToHashSet();
        foreach (var gameId in player.GameHistory.Select(g => g.Value))
        {
            if (!existingGameIds.Contains(gameId))
            {
                entity.GameHistory.Add(
                    new PlayerGameHistoryEntry { PlayerId = entity.Id, GameId = gameId }
                );
            }
        }
    }
}
