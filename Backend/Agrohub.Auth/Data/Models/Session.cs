namespace Agrohub.Auth.Data.Models;

public sealed class Session : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid DeviceId { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
}
