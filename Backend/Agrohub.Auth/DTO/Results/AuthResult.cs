namespace Agrohub.Auth.DTO.Results;

public sealed record AuthResult(string AccessToken, DateTimeOffset AccessExpiresAt, string RefreshTokenRaw, DateTimeOffset RefreshExpiresAt, Guid RefreshTokenId);
