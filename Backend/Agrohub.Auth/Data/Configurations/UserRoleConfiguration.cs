using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agrohub.Auth.Data.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.HasOne(x => x.User).WithMany(u => u.Roles).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId);
        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => x.UserId);
    }
}
