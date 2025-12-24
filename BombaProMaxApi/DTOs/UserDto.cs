namespace BombaProMaxApi.DTOs;

/// <summary>
/// Data Transfer Object for User entity.
/// Password is excluded from GET responses for security.
/// Password should only be provided when creating or updating a user's password.
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Password field - only used for create/update operations.
    /// Will be null/ignored in GET responses.
    /// </summary>
    public string? Password { get; set; }
    
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // User Permissions
    public bool CanManageUsers { get; set; }
    public bool CanManageProducts { get; set; }
    public bool CanViewReports { get; set; }
    public bool CanManageSettings { get; set; }
    public bool CanManageSales { get; set; }
    public bool CanManagePromotions { get; set; }
    public bool CanManageCustomers { get; set; }
    public bool CanManageSuppliers { get; set; }
    public bool CanManageCategories { get; set; }
    public bool ShowAcceuil { get; set; }
    public bool ShowTableauDeBord { get; set; }
    public bool ShowVente { get; set; }
    public bool EditLivreur { get; set; }
    public bool AddAchat { get; set; }
    public bool EditCiternes { get; set; }
    public bool EditPistolets { get; set; }
    public bool EditClients { get; set; }
    public bool AddBonFacturation { get; set; }
    public bool ShowDepenses { get; set; }
}
