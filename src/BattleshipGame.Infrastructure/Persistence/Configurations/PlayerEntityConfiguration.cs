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

        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();

        builder.HasIndex(p => p.Username).IsUnique();
        builder.HasIndex(p => p.IdentitySubject).IsUnique();
    }
}
