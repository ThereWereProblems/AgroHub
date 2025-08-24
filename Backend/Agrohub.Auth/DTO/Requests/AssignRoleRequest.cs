namespace Agrohub.Auth.DTO.Requests;

public sealed record AssignRoleRequest(Guid UserId, string RoleName);
