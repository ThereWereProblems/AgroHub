namespace Agrohub.Auth.Data.Models;

public sealed class User : Entity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();

    public string? Phone { get; set; }
    public bool PhoneConfirmed { get; set; }

    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }
    public int FailedAccessCount { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }
    public bool IsDeleted => DeletedAtUtc != null;

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<ExternalIdentity> ExternalIdentities { get; set; } = new List<ExternalIdentity>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
