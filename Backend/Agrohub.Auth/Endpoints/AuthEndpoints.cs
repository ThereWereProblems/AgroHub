using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agrohub.Auth.Endpoints;

public sealed class AuthEndpoints : ICarterModule
{
    private const string RefreshCookieName = "refresh_token";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("").WithTags("Auth");

        group.MapPost("/register", Register).AllowAnonymous().WithOpenApi();
        group.MapPost("/login", Login).AllowAnonymous().WithOpenApi();
        group.MapPost("/refresh", Refresh).AllowAnonymous().WithOpenApi();
        group.MapPost("/logout", Logout).RequireAuthorization().WithOpenApi();
        group.MapPost("/roles/assign", AssignRole).RequireAuthorization("admin").WithOpenApi();
        group.MapGet("/me", Me).RequireAuthorization();
    }

    // ===== Handlers wrappers =====
    private static async Task<IResult> Register([FromBody] RegisterRequest body, HttpContext ctx, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterUserCommand(body.Email, body.Password, body.Username), ct);
        SetRefreshCookie(ctx, result.RefreshTokenRaw, result.RefreshExpiresAt, IsCrossSiteSpa(ctx));
        return Results.Ok(new AuthResponse(result.AccessToken, result.AccessExpiresAt));
    }

    private static async Task<IResult> Login([FromBody] LoginRequest body, HttpContext ctx, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(body.Email, body.Password, body.DeviceId), ct);
        SetRefreshCookie(ctx, result.RefreshTokenRaw, result.RefreshExpiresAt, IsCrossSiteSpa(ctx));
        return Results.Ok(new AuthResponse(result.AccessToken, result.AccessExpiresAt));
    }

    private static async Task<IResult> Refresh(HttpContext ctx, IMediator mediator, HttpRequest req, CancellationToken ct)
    {
        var refreshCookie = req.Cookies[RefreshCookieName];
        if (string.IsNullOrWhiteSpace(refreshCookie))
            return Results.Unauthorized();

        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = req.Headers.UserAgent.ToString();

        var result = await mediator.Send(new RefreshCommand(refreshCookie, ip, ua), ct);
        SetRefreshCookie(ctx, result.RefreshTokenRaw, result.RefreshExpiresAt, IsCrossSiteSpa(ctx));
        return Results.Ok(new AuthResponse(result.AccessToken, result.AccessExpiresAt));
    }

    private static async Task<IResult> Logout([FromBody] LogoutRequest body, HttpContext ctx, IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new LogoutCommand(body.RefreshTokenId), ct);
        ClearRefreshCookie(ctx, IsCrossSiteSpa(ctx));
        return Results.NoContent();
    }

    private static async Task<IResult> AssignRole([FromBody] AssignRoleRequest body, IMediator mediator, ClaimsPrincipal user, CancellationToken ct)
    {
        Guid? by = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"), out var g) ? g : null;
        await mediator.Send(new AssignRoleCommand(body.UserId, body.RoleName, by), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Me(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        var email = user.FindFirstValue(ClaimTypes.Email);
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        return Results.Ok(new { sub, email, roles });
    }

    // ===== Cookie helpers =====
    private static void SetRefreshCookie(HttpContext ctx, string rawRefreshToken, DateTimeOffset expiresAt, bool crossSiteSpa)
    {
        var opts = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,                                   // Wymaga HTTPS
            SameSite = crossSiteSpa ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = expiresAt.UtcDateTime,
            Path = "/auth"
        };
        ctx.Response.Cookies.Append(RefreshCookieName, rawRefreshToken, opts);
    }

    private static void ClearRefreshCookie(HttpContext ctx, bool crossSiteSpa)
    {
        var opts = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = crossSiteSpa ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UnixEpoch.UtcDateTime,
            Path = "/auth"
        };
        ctx.Response.Cookies.Append(RefreshCookieName, string.Empty, opts);
    }

    private static bool IsCrossSiteSpa(HttpContext ctx)
    {
        // Jeżeli front działa na innym originie (np. http://localhost:4200), potrzebujesz SameSite=None
        // Najprościej przez konfigurację: appsettings => Auth:CrossSiteSpa = true
        return ctx.RequestServices.GetRequiredService<IConfiguration>()
                 .GetValue<bool>("Auth:CrossSiteSpa");
    }
}
