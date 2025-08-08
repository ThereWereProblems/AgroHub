using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class WebAuthnCredentialConfiguration : IEntityTypeConfiguration<WebAuthnCredential>
{
    public void Configure(EntityTypeBuilder<WebAuthnCredential> builder)
    {
        builder.ToTable("webauthn_credentials");
        builder.Property(x => x.CredentialId).HasColumnType("bytea").IsRequired();
        builder.Property(x => x.PublicKey).HasColumnType("bytea").IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CredentialId).IsUnique();
    }
}
