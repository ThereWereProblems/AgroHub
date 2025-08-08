using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net;

namespace Agrohub.Auth.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
        builder.Property(x => x.Ip)
            .HasConversion(
                toProvider => toProvider == null ? null : IPAddress.Parse(toProvider),  // string -> IPAddress
                fromProvider => fromProvider == null ? null : fromProvider.ToString())  // IPAddress -> string
            .HasColumnType("inet");
        builder.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.ReplacedBy).WithMany().HasForeignKey(x => x.ReplacedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
