namespace Agrohub.Auth.Data.Models;

public sealed class WebAuthnCredential : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public byte[] CredentialId { get; set; } = null!;
    public byte[] PublicKey { get; set; } = null!;
    public int SignCount { get; set; }
    public string? DeviceNickname { get; set; }
}