using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
