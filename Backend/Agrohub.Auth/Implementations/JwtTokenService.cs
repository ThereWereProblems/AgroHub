using Agrohub.Auth.Interfaces;
using Agrohub.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Agrohub.Auth.Implementations;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public (string token, DateTimeOffset expiresAt) CreateAccessToken(User user, IEnumerable<string> roles, Guid deviceId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = _clock.UtcNow;
        var expires = now.AddMinutes(_options.AccessMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("sid", deviceId.ToString()) // identyfikator urządzenia/sesji
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(token), expires);
    }

    public (RefreshToken entity, string rawValue) CreateRefreshToken(User user, Guid familyId, Guid deviceId, TimeSpan ttl)
    {
        var now = _clock.UtcNow;
        var raw = GenerateSecureToken(32); // 256-bit losowy token (base64url)
        var hash = HashRefresh(raw);

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            FamilyId = familyId,
            TokenHash = hash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(ttl),
            DeviceId = deviceId,
            // Ip/UserAgent ustawisz w handlerze jeśli potrzebujesz
        };

        return (entity, raw);
    }

    public byte[] HashRefresh(string rawValue)
    {
        // SHA-256 nad Base64Url stringiem -> byte[] do kolumny bytea
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(rawValue));
    }

    // ===== helpers =====
    private static string GenerateSecureToken(int bytesLength)
    {
        var bytes = new byte[bytesLength];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
