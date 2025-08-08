namespace Agrohub.Auth.Data.Models;

public sealed class TotpSecret : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public byte[] Secret { get; set; } = null!;         // zaszyfrowane
    public bool IsEnabled { get; set; }
    public DateTimeOffset? LastVerifiedAtUtc { get; set; }
    public string? RecoveryCodesHashJson { get; set; }  // hashed codes in JSON
}