using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
