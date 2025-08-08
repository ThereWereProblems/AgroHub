using Microsoft.EntityFrameworkCore;

namespace Agrohub.Auth.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
    public DbSet<TotpSecret> TotpSecrets => Set<TotpSecret>();
    public DbSet<WebAuthnCredential> WebAuthnCredentials => Set<WebAuthnCredential>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // Postgres-specific types
        model.HasPostgresExtension("citext");

        // Users
        model.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(x => x.Email).HasColumnType("citext").IsRequired();
            e.Property(x => x.Username).HasColumnType("citext");
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.DeletedAtUtc); // for partial/filtered queries
            e.HasQueryFilter(x => x.DeletedAtUtc == null);
        });

        // Roles
        model.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.Property(x => x.Name).HasColumnType("citext").IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        // UserRoles
        model.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.Roles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId);
            e.HasIndex(x => x.RoleId);
        });

        // RefreshTokens
        model.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
            e.Property(x => x.Ip).HasColumnType("inet");
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.ReplacedBy).WithMany().HasForeignKey(x => x.ReplacedById).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.FamilyId);
            e.HasIndex(x => x.ExpiresAtUtc);
        });

        // Sessions
        model.Entity<Session>(e =>
        {
            e.ToTable("sessions");
            e.HasOne(x => x.User).WithMany(u => u.Sessions).HasForeignKey(x => x.UserId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.DeviceId);
            e.HasIndex(x => x.LastSeenAtUtc);
        });

        // ExternalIdentities
        model.Entity<ExternalIdentity>(e =>
        {
            e.ToTable("external_identities");
            e.HasOne(x => x.User).WithMany(u => u.ExternalIdentities).HasForeignKey(x => x.UserId);
            e.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        });

        // TOTP
        model.Entity<TotpSecret>(e =>
        {
            e.ToTable("totp_secrets");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Secret).HasColumnType("bytea").IsRequired();
            e.Property(x => x.RecoveryCodesHashJson).HasColumnType("jsonb");
            e.HasOne(x => x.User).WithOne().HasForeignKey<TotpSecret>(x => x.UserId);
        });

        // WebAuthn
        model.Entity<WebAuthnCredential>(e =>
        {
            e.ToTable("webauthn_credentials");
            e.Property(x => x.CredentialId).HasColumnType("bytea").IsRequired();
            e.Property(x => x.PublicKey).HasColumnType("bytea").IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.CredentialId).IsUnique();
        });

        // EmailVerificationToken
        model.Entity<EmailVerificationToken>(e =>
        {
            e.ToTable("email_verification_tokens");
            e.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAtUtc);
        });

        // PasswordResetToken
        model.Entity<PasswordResetToken>(e =>
        {
            e.ToTable("password_reset_tokens");
            e.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAtUtc);
        });

        // AuditLogs
        model.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.MetadataJson).HasColumnType("jsonb");
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Action);
            e.HasIndex(x => x.CreatedAtUtc);
        });

        // LoginAttempts
        model.Entity<LoginAttempt>(e =>
        {
            e.ToTable("login_attempts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Ip).HasColumnType("inet");
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Email);
            e.HasIndex(x => x.CreatedAtUtc);
        });

        // OutboxMessages
        model.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasKey(x => x.Id);
            e.Property(x => x.PayloadJson).HasColumnType("jsonb");
            e.HasIndex(x => x.CreatedAtUtc);
            e.HasIndex(x => x.ProcessedAtUtc);
        });
    }

    public override int SaveChanges()
    {
        TouchTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void TouchTimestamps()
    {
        var entries = ChangeTracker.Entries<Entity>();
        var now = DateTimeOffset.UtcNow;
        foreach (var e in entries)
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedAtUtc = now;
                e.Entity.UpdatedAtUtc = now;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Entity.UpdatedAtUtc = now;
            }
        }
    }
}
