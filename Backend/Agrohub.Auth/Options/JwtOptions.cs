namespace Agrohub.Auth.Options;

public sealed class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Key { get; init; }
    public int AccessMinutes { get; init; } = 10;
}
