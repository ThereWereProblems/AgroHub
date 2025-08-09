namespace Agrohub.Auth.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(User user, string confirmationUrl, CancellationToken ct);
}
