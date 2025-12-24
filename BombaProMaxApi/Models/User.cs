namespace BombaProMaxApi.Models;

using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

public class User
{
    // User Properties
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; } = false;
    public bool IsSuperAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int createdBy { get; set; }
    public int updatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;



    // User Permissions
    public bool CanManageUsers { get; set; } = false;
    public bool CanManageProducts { get; set; } = false;
    public bool CanViewReports { get; set; } = false;
    public bool CanManageSettings { get; set; } = false;
    public bool CanManageSales { get; set; } = false;
    public bool CanManagePromotions { get; set; } = false;
    public bool CanManageCustomers { get; set; } = false;
    public bool CanManageSuppliers { get; set; } = false;
    public bool CanManageCategories { get; set; } = false;
    public bool ShowAcceuil { get; set; } = false;
    public bool ShowTableauDeBord { get; set; } = false;
    public bool ShowVente { get; set; } = false;
    public bool EditLivreur { get; set; } = false;
    public bool AddAchat { get; set; } = false;
    public bool EditCiternes { get; set; } = false;
    public bool EditPistolets { get; set; } = false;
    public bool EditClients { get; set; } = false;
    public bool AddBonFacturation { get; set; } = false;
    public bool ShowDepenses { get; set; } = false;
}

