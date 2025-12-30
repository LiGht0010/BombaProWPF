using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Achat
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public string? Numero { get; set; }

    public DateOnly Date { get; set; }

    public int? FournisseurID { get; set; }

    public int? ProduitID { get; set; }

    public int? Quantite { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Coût Total")]
    public decimal? Cout { get; set; }

    // New field to store the purchase price at time of purchase
    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Prix d'Achat Unitaire")]
    public decimal? PrixAchatUnitaire { get; set; }

    public int? ChauffeurID { get; set; }

    public int? CamionID { get; set; }
    
    public bool? LivraisonDefectueuse { get; set; }

    [ForeignKey("CamionID")]
    [InverseProperty("Achats")]
    public virtual Camion? Camion { get; set; }

    [ForeignKey("ChauffeurID")]
    [InverseProperty("Achats")]
    public virtual Chauffeur? Chauffeur { get; set; }

    [ForeignKey("FournisseurID")]
    [InverseProperty("Achats")]
    public virtual Fournisseur? Fournisseur { get; set; }

    [ForeignKey("ProduitID")]
    [InverseProperty("Achats")]
    public virtual Produit? Produit { get; set; }

    // New navigation property for fuel allocations
    [InverseProperty("Achat")]
    public virtual ICollection<AchatAllocation> AchatAllocations { get; set; } = new List<AchatAllocation>();

    // Audit fields
    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}
// Add this method to the Achat class
public partial class Achat
{
    // Category IDs as seeded in AppDbContext
    private const int CarburantCategoryId = 1;

    /// <summary>
    /// Updates product stock for non-fuel purchases (Lubricants, Articles, etc.)
    /// Stock is updated for all product categories EXCEPT CARBURANT (ID = 1).
    /// </summary>
    /// <returns>True if stock was updated, false if not applicable (carburant or missing data)</returns>
    public bool UpdateProductStock()
    {
        // Only update stock for non-fuel products
        if (Produit?.CategorieID != null)
        {
            // Skip fuel products (CategorieID = 1) - their stock is handled via reservoir allocations
            if (Produit.CategorieID == CarburantCategoryId)
            {
                return false;
            }

            // Update stock for all other categories (LUBRIFIANT = 2, ARTICLE = 3, etc.)
            if (Quantite.HasValue && Quantite.Value > 0)
            {
                Produit.Stock = (Produit.Stock ?? 0) + Quantite.Value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Reverses stock update when purchase is deleted or modified.
    /// Applies to all non-fuel product categories.
    /// </summary>
    /// <returns>True if stock was reversed, false if not applicable</returns>
    public bool ReverseProductStock()
    {
        if (Produit?.CategorieID != null)
        {
            // Skip fuel products (CategorieID = 1) - their stock is handled via reservoir allocations
            if (Produit.CategorieID == CarburantCategoryId)
            {
                return false;
            }

            // Reverse stock for all other categories
            if (Quantite.HasValue && Quantite.Value > 0)
            {
                Produit.Stock = Math.Max(0, (Produit.Stock ?? 0) - Quantite.Value);
                return true;
            }
        }

        return false;
    }
}
