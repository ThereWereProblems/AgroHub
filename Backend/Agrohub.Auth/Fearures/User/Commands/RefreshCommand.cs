using Agrohub.Auth.DTO;
using Agrohub.Auth.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Agrohub.Auth.Fearures.User.Commands;

public sealed record RefreshCommand(string RefreshTokenFromCookie, string Ip, string? UserAgent) : IRequest<AuthResult>;

public sealed class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator()
    {
        RuleFor(x => x.RefreshTokenFromCookie).NotEmpty();
        RuleFor(x => x.Ip).NotEmpty();
    }
}

public sealed class RefreshHandler(IdentityDbContext db, ITokenService tokens, IClock clock) : IRequestHandler<RefreshCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshCommand cmd, CancellationToken ct)
    {
        var hash = tokens.HashRefresh(cmd.RefreshTokenFromCookie);
        var rt = await db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (rt is null || rt.RevokedAtUtc is not null || rt.ExpiresAtUtc <= clock.UtcNow)
            throw new UnauthorizedAccessException("Refresh nieprawidłowy lub wygasły.");

        if (rt.ReplacedById is not null)
        {
            await RevokeFamily(rt.FamilyId, "REUSE_DETECTED", ct);
            throw new UnauthorizedAccessException("Wykryto ponowne użycie refresh tokena.");
        }

        var roles = await db.UserRoles.Where(ur => ur.UserId == rt.UserId)
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(ct);

        var (access, accessExp) = tokens.CreateAccessToken(rt.User, roles, rt.DeviceId);
        var (newRt, newRaw) = tokens.CreateRefreshToken(rt.User, rt.FamilyId, rt.DeviceId, TimeSpan.FromDays(7));
        newRt.Ip = cmd.Ip;
        newRt.UserAgent = cmd.UserAgent;

        rt.RevokedAtUtc = clock.UtcNow;
        rt.RevokedReason = "ROTATED";
        rt.ReplacedById = newRt.Id;

        db.RefreshTokens.Add(newRt);
        await db.SaveChangesAsync(ct);

        return new AuthResult(access, accessExp, newRaw, newRt.ExpiresAtUtc, newRt.Id);
    }

    private async Task RevokeFamily(Guid familyId, string reason, CancellationToken ct)
    {
        var now = clock.UtcNow;
        await db.RefreshTokens
            .Where(x => x.FamilyId == familyId && x.RevokedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RevokedAtUtc, now)
                .SetProperty(x => x.RevokedReason, reason), ct);
    }
}

