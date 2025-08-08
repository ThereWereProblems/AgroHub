namespace Agrohub.Auth.Data.Models;

public sealed class RefreshToken : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid FamilyId { get; set; }
    public byte[] TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public Guid? ReplacedById { get; set; }
    public RefreshToken? ReplacedBy { get; set; }

    public Guid DeviceId { get; set; }
    public string? Ip { get; set; }          // mapped to inet
    public string? UserAgent { get; set; }
}