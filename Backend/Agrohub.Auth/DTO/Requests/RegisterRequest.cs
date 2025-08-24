namespace Agrohub.Auth.DTO.Requests;

public sealed record RegisterRequest(string Email, string Password, string? Username);