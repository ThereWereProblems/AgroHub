using Agrohub.Common.Events;
using Agrohub.EmailSender.Features.Email.Commands;
using MassTransit;
using MediatR;

namespace Agrohub.EmailSender.Features.Email.EventHandlers;

public class SendEmailHandler(ISender sender, ILogger<SendEmailHandler> logger) : IConsumer<SendEmailEvent>
{
    public async Task Consume(ConsumeContext<SendEmailEvent> context)
    {
        logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        var emailEvent = context.Message;
        var command = new SendEmailCommand(emailEvent.To, emailEvent.Subject, emailEvent.HtmlBody, emailEvent.CorrelationId);
        await sender.Send(command);
    }
}