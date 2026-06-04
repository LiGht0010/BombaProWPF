namespace FourniProApi.Models;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public bool IsAdmin { get; set; } = false;
    public bool IsSuperAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<UserPermission> UserPermissions { get; set; } = [];
}
