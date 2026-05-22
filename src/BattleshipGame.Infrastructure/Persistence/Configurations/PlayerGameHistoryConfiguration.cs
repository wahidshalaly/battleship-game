using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BattleshipGame.Infrastructure.Persistence.Configurations;

internal class PlayerGameHistoryConfiguration : IEntityTypeConfiguration<PlayerGameHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PlayerGameHistoryEntry> builder)
    {
        builder.ToTable("player_game_history");

        builder.HasKey(h => new { h.PlayerId, h.GameId });
        builder.Property(h => h.PlayerId).HasColumnName("player_id");
        builder.Property(h => h.GameId).HasColumnName("game_id");
    }
}
