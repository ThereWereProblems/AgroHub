namespace Agrohub.Auth.DTO;

public sealed record AuthResult(string AccessToken, DateTimeOffset AccessExpiresAt, string RefreshTokenRaw, DateTimeOffset RefreshExpiresAt, Guid RefreshTokenId);