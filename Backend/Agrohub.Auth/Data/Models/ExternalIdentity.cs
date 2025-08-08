namespace Agrohub.Auth.Data.Models;

public sealed class ExternalIdentity : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Provider { get; set; } = null!;       // "google", "facebook"
    public string ProviderUserId { get; set; } = null!;
    public string? EmailAtProvider { get; set; }
    public DateTimeOffset LinkedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
