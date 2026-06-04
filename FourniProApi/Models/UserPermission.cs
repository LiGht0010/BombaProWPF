namespace FourniProApi.Models;

/// <summary>Join table — binds a User to a Permission.</summary>
public class UserPermission
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
