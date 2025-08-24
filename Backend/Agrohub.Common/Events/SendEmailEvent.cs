namespace Agrohub.Common.Events;

public sealed record SendEmailEvent(
    string MessageId,          // GUID jako string
    string To,
    string Subject,
    string HtmlBody,
    string? CorrelationId = null,
    DateTimeOffset? OccurredAt = null);
