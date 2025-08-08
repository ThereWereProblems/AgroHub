using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        builder.HasOne(x => x.User).WithMany(u => u.Sessions).HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.DeviceId);
        builder.HasIndex(x => x.LastSeenAtUtc);
    }
}
