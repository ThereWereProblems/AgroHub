using Agrohub.Auth.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Agrohub.Auth.Fearures.User.Commands;

public sealed record AssignRoleCommand(Guid UserId, string RoleName, Guid? AssignedBy) : IRequest<Unit>;

public sealed class AssignRoleValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AssignRoleHandler(IdentityDbContext db, IClock clock) : IRequestHandler<AssignRoleCommand, Unit>
{
    public async Task<Unit> Handle(AssignRoleCommand cmd, CancellationToken ct)
    {
        var userExists = await db.Users.AnyAsync(u => u.Id == cmd.UserId && u.DeletedAtUtc == null, ct);
        if (!userExists) throw new KeyNotFoundException("Użytkownik nie istnieje.");

        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == cmd.RoleName, ct)
                   ?? throw new KeyNotFoundException("Rola nie istnieje.");

        var already = await db.UserRoles.AnyAsync(ur => ur.UserId == cmd.UserId && ur.RoleId == role.Id, ct);
        if (already) return Unit.Value;

        db.UserRoles.Add(new UserRole { UserId = cmd.UserId, RoleId = role.Id, AssignedBy = cmd.AssignedBy, AssignedAtUtc = clock.UtcNow });

        db.AuditLogs.Add(new AuditLog { UserId = cmd.AssignedBy, Action = "ROLE_ASSIGNED", SubjectId = cmd.UserId, MetadataJson = $"{{\"role\":\"{cmd.RoleName}\"}}", CreatedAtUtc = clock.UtcNow });

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
