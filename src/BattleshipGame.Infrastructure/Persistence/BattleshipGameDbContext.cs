using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.Infrastructure.Persistence;

public class BattleshipGameDbContext(DbContextOptions<BattleshipGameDbContext> options)
    : DbContext(options)
{
    internal DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    internal DbSet<PlayerGameHistoryEntry> PlayerGameHistory => Set<PlayerGameHistoryEntry>();
    internal DbSet<GameEntity> Games => Set<GameEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BattleshipGameDbContext).Assembly);
    }
}
