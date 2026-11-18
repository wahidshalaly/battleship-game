using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.Infrastructure.Persistence.Repositories;

internal class PlayerRepository(BattleshipGameDbContext context) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(PlayerId playerId, CancellationToken ct)
    {
        var entity = await context.Players.FirstOrDefaultAsync(p => p.Id == playerId.Value, ct);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<Player> GetByIdOrThrowAsync(PlayerId playerId, CancellationToken ct)
    {
        var entity =
            await context.Players.FirstOrDefaultAsync(p => p.Id == playerId.Value, ct)
            ?? throw new Domain.Exceptions.PlayerNotFoundException(playerId);

        return MapToDomain(entity);
    }

    public async Task<PlayerId> SaveAsync(Player player, CancellationToken ct)
    {
        // A player's username and identity subject never change, so only new players are written.
        if (!await context.Players.AnyAsync(p => p.Id == player.Id.Value, ct))
            await context.Players.AddAsync(MapToEntity(player), ct);

        return player.Id;
    }

    public async Task<Player?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(username))
            return null;

        var entity = await context.Players.FirstOrDefaultAsync(
            p => p.Username.ToLower() == username.ToLower(),
            ct
        );

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct) =>
        !string.IsNullOrEmpty(username)
        && await context.Players.AnyAsync(p => p.Username.ToLower() == username.ToLower(), ct);

    public async Task<Player?> GetByIdentitySubjectAsync(
        string identitySubject,
        CancellationToken ct
    )
    {
        if (string.IsNullOrEmpty(identitySubject))
            return null;

        var entity = await context.Players.FirstOrDefaultAsync(
            p => p.IdentitySubject == identitySubject,
            ct
        );

        return entity is null ? null : MapToDomain(entity);
    }

    private static Player MapToDomain(PlayerEntity entity) =>
        new(new PlayerId(entity.Id), entity.Username, entity.IdentitySubject);

    private static PlayerEntity MapToEntity(Player player) =>
        new()
        {
            Id = player.Id.Value,
            Username = player.Username,
            IdentitySubject = player.IdentitySubject,
        };
}
