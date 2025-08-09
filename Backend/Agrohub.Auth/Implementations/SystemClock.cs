using Agrohub.Auth.Interfaces;

namespace Agrohub.Auth.Implementations;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
