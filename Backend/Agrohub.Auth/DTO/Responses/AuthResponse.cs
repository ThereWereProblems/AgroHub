namespace Agrohub.Auth.DTO.Responses;

public sealed record AuthResponse(string AccessToken, DateTimeOffset AccessExpiresAt);
