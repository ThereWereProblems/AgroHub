namespace Agrohub.Auth.Interfaces;

public interface ITokenService
{
    // Tworzy krótkożyjący JWT (np. 10 min)
    (string token, DateTimeOffset expiresAt) CreateAccessToken(User user, IEnumerable<string> roles, Guid deviceId);
    // Tworzy i zapisuje refresh (rotacja i rodzinę obsłużymy w handlerze)
    (RefreshToken entity, string rawValue) CreateRefreshToken(User user, Guid familyId, Guid deviceId, TimeSpan ttl);
    byte[] HashRefresh(string rawValue); // dla porównania przy /refresh
}
