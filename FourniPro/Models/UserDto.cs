namespace FourniPro.Models;

/// <summary>
/// Mirrors FourniProApi.DTOs.UserDto — deserialized from API login response.
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Password { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Permission names granted to this user (e.g. "ventes.view").</summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary>Returns true if the user has the specified permission name.</summary>
    public bool HasPermission(string permissionName) =>
        Permissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);

    public string RoleDisplay => IsSuperAdmin ? "Superviseur" : IsAdmin ? "Administrateur" : "Utilisateur";
    public string RoleText => IsSuperAdmin ? "SUPER ADMIN" : IsAdmin ? "ADMIN" : "UTILISATEUR";
}

