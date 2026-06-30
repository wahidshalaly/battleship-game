using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BattleshipGame.Infrastructure.Persistence.Configurations;

internal class PlayerEntityConfiguration : IEntityTypeConfiguration<PlayerEntity>
{
    public void Configure(EntityTypeBuilder<PlayerEntity> builder)
    {
        builder.ToTable("players");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Username).HasColumnName("username").IsRequired();
        builder.Property(p => p.IdentitySubject).HasColumnName("identity_subject").IsRequired();
        builder.Property(p => p.ActiveGameId).HasColumnName("active_game_id");

        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();

        builder.HasIndex(p => p.Username).IsUnique();
        builder.HasIndex(p => p.IdentitySubject).IsUnique();
        builder.HasIndex(p => p.ActiveGameId).IsUnique().HasFilter("active_game_id IS NOT NULL");

        builder
            .HasMany(p => p.GameHistory)
            .WithOne(h => h.Player)
            .HasForeignKey(h => h.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
