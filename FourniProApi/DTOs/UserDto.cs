namespace FourniProApi.DTOs;

/// <summary>
/// Returned by the API after a successful login.
/// Password is never included in responses.
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;

    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The list of permission names granted to this user (e.g. "ventes.view", "achats.create").
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}
