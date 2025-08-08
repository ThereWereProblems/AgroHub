namespace Agrohub.Auth.Data.Models;

public sealed class LoginAttempt
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? Ip { get; set; }                      // inet
    public bool Succeeded { get; set; }
    public string? Reason { get; set; }                  // BAD_PASSWORD, LOCKED_OUT...
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}