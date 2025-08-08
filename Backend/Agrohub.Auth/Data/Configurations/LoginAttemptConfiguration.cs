using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net;

namespace Agrohub.Auth.Data.Configurations;

public sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("login_attempts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Ip)
            .HasConversion(
                toProvider => toProvider == null ? null : IPAddress.Parse(toProvider),  // string -> IPAddress
                fromProvider => fromProvider == null ? null : fromProvider.ToString())  // IPAddress -> string
            .HasColumnType("inet");
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.CreatedAtUtc);

        builder.Property(x => x.Ip)
    
    .HasColumnType("inet");
    }
}
