namespace Agrohub.Auth.Data.Models;

public sealed class OutboxMessage
{
    public long Id { get; set; }
    public string AggregateType { get; set; } = null!;   // "User"
    public Guid AggregateId { get; set; }
    public string Type { get; set; } = null!;            // "UserRegistered"
    public string PayloadJson { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAtUtc { get; set; }
}