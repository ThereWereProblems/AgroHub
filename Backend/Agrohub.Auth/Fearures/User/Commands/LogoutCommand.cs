using Agrohub.Auth.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Agrohub.Auth.Fearures.User.Commands;

public sealed record LogoutCommand(Guid RefreshTokenId) : IRequest<Unit>;

public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshTokenId).NotEmpty();
    }
}

public sealed class LogoutHandler(IdentityDbContext db, IClock clock) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var rt = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Id == cmd.RefreshTokenId, ct);
        if (rt is null) return Unit.Value;

        rt.RevokedAtUtc = clock.UtcNow;
        rt.RevokedReason = "LOGOUT";

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
