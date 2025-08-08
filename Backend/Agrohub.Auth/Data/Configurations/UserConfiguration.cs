using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.Property(x => x.Email).HasColumnType("citext").IsRequired();
        builder.Property(x => x.Username).HasColumnType("citext");
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.DeletedAtUtc);
        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
