using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb");
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.ProcessedAtUtc);
    }
}
