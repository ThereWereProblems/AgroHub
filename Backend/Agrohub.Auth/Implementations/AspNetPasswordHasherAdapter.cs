using Agrohub.Auth.Interfaces;

namespace Agrohub.Auth.Implementations;

public sealed class AspNetPasswordHasherAdapter : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _impl = new();

    public string Hash(string password) => _impl.HashPassword(new User(), password);

    public bool Verify(string hash, string password)
        => _impl.VerifyHashedPassword(new User(), hash, password)
           is Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success
            or Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded;
}
