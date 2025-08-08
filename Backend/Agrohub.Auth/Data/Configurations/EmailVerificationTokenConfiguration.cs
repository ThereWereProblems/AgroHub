using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");
        builder.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
