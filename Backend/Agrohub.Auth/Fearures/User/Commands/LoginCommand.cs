using Agrohub.Auth.DTO;
using Agrohub.Auth.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Agrohub.Auth.Fearures.User.Commands;

public sealed record LoginCommand(string Email, string Password, Guid DeviceId) : IRequest<AuthResult>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
    }
}

public sealed class LoginHandler(IdentityDbContext db, IPasswordHasher hasher, ITokenService tokens, IClock clock) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == cmd.Email && u.DeletedAtUtc == null, ct);
        if (user is null)
        {
            await LogAttempt(null, cmd.Email, false, "USER_NOT_FOUND", ct);
            throw new UnauthorizedAccessException("Nieprawidłowe dane logowania.");
        }

        if (user.LockoutEnabled && user.LockoutEndUtc > clock.UtcNow)
            throw new UnauthorizedAccessException("Konto zablokowane.");

        if (user.PasswordHash is null || !hasher.Verify(user.PasswordHash, cmd.Password))
        {
            user.FailedAccessCount++;
            if (user.FailedAccessCount >= 5)
            {
                user.LockoutEnabled = true;
                user.LockoutEndUtc = clock.UtcNow.AddMinutes(15);
            }
            await db.SaveChangesAsync(ct);
            await LogAttempt(user.Id, cmd.Email, false, "BAD_PASSWORD", ct);
            throw new UnauthorizedAccessException("Nieprawidłowe dane logowania.");
        }

        user.FailedAccessCount = 0;
        await db.SaveChangesAsync(ct);

        var roles = await db.UserRoles.Where(ur => ur.UserId == user.Id)
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(ct);

        var familyId = Guid.NewGuid();
        var (access, accessExp) = tokens.CreateAccessToken(user, roles, cmd.DeviceId);
        var (refreshEntity, refreshRaw) = tokens.CreateRefreshToken(user, familyId, cmd.DeviceId, TimeSpan.FromDays(7));
        db.RefreshTokens.Add(refreshEntity);

        db.AuditLogs.Add(new AuditLog { UserId = user.Id, Action = "USER_LOGIN_SUCCESS", SubjectId = user.Id, CreatedAtUtc = clock.UtcNow });
        await db.SaveChangesAsync(ct);

        return new AuthResult(access, accessExp, refreshRaw, refreshEntity.ExpiresAtUtc, refreshEntity.Id);
    }

    private Task LogAttempt(Guid? userId, string email, bool ok, string? reason, CancellationToken ct)
        => db.LoginAttempts.AddAsync(new LoginAttempt { UserId = userId, Email = email, Succeeded = ok, Reason = reason, CreatedAtUtc = clock.UtcNow }, ct).AsTask();
}

