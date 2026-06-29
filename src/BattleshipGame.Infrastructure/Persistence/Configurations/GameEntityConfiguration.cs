using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BattleshipGame.Infrastructure.Persistence.Configurations;

internal class GameEntityConfiguration : IEntityTypeConfiguration<GameEntity>
{
    public void Configure(EntityTypeBuilder<GameEntity> builder)
    {
        builder.ToTable("games");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.PlayerId).HasColumnName("player_id").IsRequired();
        builder.Property(g => g.BoardSize).HasColumnName("board_size").IsRequired();
        builder.Property(g => g.OpponentStrategy).HasColumnName("opponent_strategy").IsRequired();
        builder.Property(g => g.State).HasColumnName("state").IsRequired();
        builder.Property(g => g.TargetSide).HasColumnName("target_side").IsRequired();
        builder.Property(g => g.WinnerSide).HasColumnName("winner_side").IsRequired();
        builder.Property(g => g.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(g => g.LastUpdatedAt).HasColumnName("last_updated_at").IsRequired();

        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();

        builder.OwnsOne(
            g => g.OwnBoard,
            board =>
            {
                board.ToJson("own_board");
                board.OwnsMany(b => b.Ships);
                board.OwnsMany(b => b.Cells);
            }
        );

        builder.OwnsOne(
            g => g.OppBoard,
            board =>
            {
                board.ToJson("opp_board");
                board.OwnsMany(b => b.Ships);
                board.OwnsMany(b => b.Cells);
            }
        );
    }
}
