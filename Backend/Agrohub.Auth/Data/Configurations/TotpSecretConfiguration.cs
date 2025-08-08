using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class TotpSecretConfiguration : IEntityTypeConfiguration<TotpSecret>
{
    public void Configure(EntityTypeBuilder<TotpSecret> builder)
    {
        builder.ToTable("totp_secrets");
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.Secret).HasColumnType("bytea").IsRequired();
        builder.Property(x => x.RecoveryCodesHashJson).HasColumnType("jsonb");
        builder.HasOne(x => x.User).WithOne().HasForeignKey<TotpSecret>(x => x.UserId);
    }
}
