namespace Agrohub.Auth.Data.Models;

public sealed class Role : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}
