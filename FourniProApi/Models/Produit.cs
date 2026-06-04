using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FourniProApi.Models;

public class Produit
{
    [Key]
    public int ProduitId { get; set; }

    [StringLength(20)]
    public string NumeroProduit { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrixAchat { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrixHT { get; set; }

    /// <summary>TVA percentage — defaults to 20%.</summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? TVA { get; set; } = 20;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrixTTC { get; set; }

    public int? Stock { get; set; }

    public int? StockMinimum { get; set; }

    public int? DelaiDeLivraison { get; set; }

    // Audit
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Calculated — not stored
    [NotMapped]
    public decimal? MargeBeneficiaire =>
        PrixHT.HasValue && PrixAchat.HasValue
            ? PrixHT.Value - PrixAchat.Value
            : null;

    [NotMapped]
    public decimal? MargePourcentage =>
        PrixAchat is > 0 && MargeBeneficiaire.HasValue
            ? Math.Round(MargeBeneficiaire.Value / PrixAchat.Value * 100, 2)
            : null;

    /// <summary>Recalculates PrixTTC from PrixHT and TVA.</summary>
    public void CalculatePrixTTC()
    {
        if (PrixHT.HasValue && TVA.HasValue)
            PrixTTC = Math.Round(PrixHT.Value * (1 + TVA.Value / 100), 2);
    }

    /// <summary>Recalculates PrixHT from PrixTTC and TVA.</summary>
    public void CalculatePrixHT()
    {
        if (PrixTTC.HasValue && TVA.HasValue)
            PrixHT = Math.Round(PrixTTC.Value / (1 + TVA.Value / 100), 2);
    }
}
