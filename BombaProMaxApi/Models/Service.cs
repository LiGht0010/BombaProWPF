using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Service
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public string? Numero { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Prix { get; set; }

    // Navigation property for CreditTransactions
    [InverseProperty("Service")]
    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    // Navigation property for ElementsFactures
    [InverseProperty("Service")]
    public virtual ICollection<ElementsFacture> ElementsFactures { get; set; } = new List<ElementsFacture>();

    // Navigation property for BonLivraisonDetails
    [InverseProperty("Service")]
    public virtual ICollection<BonLivraisonDetails> BonLivraisonDetails { get; set; } = new List<BonLivraisonDetails>();

    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}
