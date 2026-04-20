using System.Collections.ObjectModel;

namespace BombaProMaxWPF.Models;

/// <summary>
/// Data Transfer Object for User entity.
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Password field - only used for create/update operations.
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

    // ????????????????????????????????????????????????????????????????
    // COMPUTED DISPLAY PROPERTIES
    // ????????????????????????????????????????????????????????????????

    /// <summary>
    /// Gets the role display name based on admin flags.
    /// </summary>
    public string RoleDisplay => IsSuperAdmin ? "Superviseur" : IsAdmin ? "Administrateur" : "Utilisateur";

    /// <summary>
    /// Gets the role text for badges (uppercase).
    /// </summary>
    public string RoleText => IsSuperAdmin ? "SUPER ADMIN" : IsAdmin ? "ADMIN" : "UTILISATEUR";

    /// <summary>
    /// Gets the role color for badges.
    /// </summary>
    public string RoleColor => IsSuperAdmin ? "#9C27B0" : IsAdmin ? "#EF5350" : "#2196F3";

    /// <summary>
    /// Gets the status display text.
    /// </summary>
    public string StatusDisplay => IsActive ? "Actif" : "Inactif";

    /// <summary>
    /// Gets the status text for badges.
    /// </summary>
    public string StatusText => IsActive ? "? ACTIF" : "? INACTIF";

    /// <summary>
    /// Gets the status color for badges.
    /// </summary>
    public string StatusColor => IsActive ? "#4CAF50" : "#9E9E9E";

    /// <summary>
    /// Gets the list of permissions as displayable strings.
    /// </summary>
    public ObservableCollection<string> Permissions
    {
        get
        {
            var permissions = new ObservableCollection<string>();
            
            if (CanManageUsers) permissions.Add("Gérer les utilisateurs");
            if (CanManageProducts) permissions.Add("Gérer les produits");
            if (CanManageSales) permissions.Add("Gérer les ventes");
            if (CanManageCustomers) permissions.Add("Gérer les clients");
            if (CanManageSuppliers) permissions.Add("Gérer les fournisseurs");
            if (CanManageCategories) permissions.Add("Gérer les catégories");
            if (CanManagePromotions) permissions.Add("Gérer les promotions");
            if (CanManageSettings) permissions.Add("Gérer les paramètres");
            if (CanViewReports) permissions.Add("Voir les rapports");
            if (ShowAcceuil) permissions.Add("Afficher Accueil");
            if (ShowTableauDeBord) permissions.Add("Afficher Tableau de bord");
            if (ShowVente) permissions.Add("Afficher Ventes");
            if (ShowDepenses) permissions.Add("Afficher Dépenses");
            if (EditLivreur) permissions.Add("Éditer Livreurs");
            if (EditCiternes) permissions.Add("Éditer Citernes");
            if (EditPistolets) permissions.Add("Éditer Pistolets");
            if (EditClients) permissions.Add("Éditer Clients");
            if (AddAchat) permissions.Add("Ajouter Achats");
            if (AddBonFacturation) permissions.Add("Ajouter Bon de facturation");

            return permissions;
        }
    }
}
