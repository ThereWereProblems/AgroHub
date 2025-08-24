namespace Agrohub.Auth.DTO.Requests;

public sealed record LoginRequest(string Email, string Password, Guid DeviceId);
