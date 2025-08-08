namespace Agrohub.Auth.Data.Models;

public sealed class AuditLog
{
    public long Id { get; set; }                         // bigserial
    public Guid? UserId { get; set; }                    // może być null dla system
    public string Action { get; set; } = null!;          // USER_LOGIN_SUCCESS etc.
    public Guid? SubjectId { get; set; }
    public string? MetadataJson { get; set; }            // jsonb
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}