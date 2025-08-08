using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class ExternalIdentityConfiguration : IEntityTypeConfiguration<ExternalIdentity>
{
    public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
    {
        builder.ToTable("external_identities");
        builder.HasOne(x => x.User).WithMany(u => u.ExternalIdentities).HasForeignKey(x => x.UserId);
        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
    }
}
