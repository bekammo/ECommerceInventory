namespace ECommerceInventory.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public ICollection<Session> Sessions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
