using Agrohub.Auth.DTO;
using Agrohub.Auth.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Agrohub.Auth.Fearures.User.Commands;

public sealed record RegisterUserCommand(string Email, string Password, string? Username) : IRequest<AuthResult>;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Hasło musi zawierać wielką literę.")
            .Matches("[a-z]").WithMessage("Hasło musi zawierać małą literę.")
            .Matches("\\d").WithMessage("Hasło musi zawierać cyfrę.");
        RuleFor(x => x.Username)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.-]*$").When(x => !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("Username może zawierać tylko litery/cyfry/_ . -");
    }
}

public sealed class RegisterUserHandler(IdentityDbContext db, IPasswordHasher hasher, ITokenService tokens, IClock clock) : IRequestHandler<RegisterUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var emailExists = await db.Users.AnyAsync(u => u.Email == cmd.Email && u.DeletedAtUtc == null, ct);
        if (emailExists) throw new InvalidOperationException("Email już istnieje.");

        var user = new Data.Models.User
        {
            Id = Guid.NewGuid(),
            Email = cmd.Email,
            Username = string.IsNullOrWhiteSpace(cmd.Username) ? null : cmd.Username,
            PasswordHash = hasher.Hash(cmd.Password),
            CreatedAtUtc = clock.UtcNow,
            UpdatedAtUtc = clock.UtcNow,
            SecurityStamp = Guid.NewGuid()
        };

        db.Users.Add(user);

        // Domyślna rola (opcjonalnie)
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "user", ct);
        if (role is not null)
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, AssignedAtUtc = clock.UtcNow });

        // Tokeny
        var familyId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var (access, accessExp) = tokens.CreateAccessToken(user, await GetRoles(user.Id, ct), deviceId);
        var (refreshEntity, refreshRaw) = tokens.CreateRefreshToken(user, familyId, deviceId, ttl: TimeSpan.FromDays(7));
        db.RefreshTokens.Add(refreshEntity);

        // Audyt
        db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "USER_REGISTERED",
            SubjectId = user.Id,
            CreatedAtUtc = clock.UtcNow
        });

        await db.SaveChangesAsync(ct);

        return new AuthResult(access, accessExp, refreshRaw, refreshEntity.ExpiresAtUtc, refreshEntity.Id);
    }

    private async Task<IEnumerable<string>> GetRoles(Guid userId, CancellationToken ct) =>
        await db.UserRoles.Where(ur => ur.UserId == userId)
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(ct);
    
}
