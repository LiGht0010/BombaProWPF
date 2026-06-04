namespace FourniProApi.Models;

public class Permission
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<UserPermission> UserPermissions { get; set; } = [];
}
