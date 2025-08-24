namespace Agrohub.EmailSender.Options;

public sealed class MailOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string User { get; init; }
    public required string Pass { get; init; }
}