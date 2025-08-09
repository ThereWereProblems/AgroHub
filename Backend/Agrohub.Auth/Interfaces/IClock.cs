namespace Agrohub.Auth.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
