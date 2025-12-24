using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

public class BonLivraison
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(20)]
    public string NumeroBL { get; set; } = null!;

    public DateOnly DateBL { get; set; }

    [Required]
    public int ClientID { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal MontantTotal { get; set; }

    public bool EstFacture { get; set; } = false;

    [StringLength(255)]
    public string? Notes { get; set; }

    // Audit fields
    public int? AjoutePar { get; set; }
    public DateTime? DateCreation { get; set; }
    public int? ModifiePar { get; set; }
    public DateTime? DateModification { get; set; }

    // Navigation properties
    [ForeignKey("ClientID")]
    [InverseProperty("BonsLivraison")]
    public virtual Client Client { get; set; } = null!;

    [InverseProperty("BonLivraison")]
    public virtual ICollection<BonLivraisonDetails> Details { get; set; } = new List<BonLivraisonDetails>();

    [InverseProperty("BonLivraison")]
    public virtual ICollection<FactureBonLivraison> FactureBonLivraisons { get; set; } = new List<FactureBonLivraison>();

    [InverseProperty("BonLivraison")]
    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();
}
