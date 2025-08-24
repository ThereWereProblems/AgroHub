using Agrohub.EmailSender.Options;
using MediatR;
using System.Net;
using System.Net.Mail;

namespace Agrohub.EmailSender.Features.Email.Commands;

public sealed record SendEmailCommand(string To, string Subject, string HtmlBody, string? CorrelationId = null) : IRequest<Unit>;

public sealed class SendEmailHandler(Microsoft.Extensions.Options.IOptions<MailOptions> options) : IRequestHandler<SendEmailCommand, Unit>
{
    public async Task<Unit> Handle(SendEmailCommand cmd, CancellationToken ct)
    {
        var op = options.Value;

        using var message = new MailMessage(op.User, cmd.To, cmd.Subject, cmd.HtmlBody);
        message.IsBodyHtml = true;

        using var client = new SmtpClient(op.Host, op.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(op.User, op.Pass)
        };

        client.Send(message);

        return Unit.Value;
    }
}
