using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Facture
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public string? NumeroFacture { get; set; }

    public DateOnly? DateFacture { get; set; }

    public int? ClientID { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? MontantTotal { get; set; }

    [StringLength(50)]
    public string? Statut { get; set; }

    public int? MoyenPaiementID { get; set; }

    public DateOnly? DatePaiement { get; set; }

    [ForeignKey("ClientID")]
    [InverseProperty("Factures")]
    public virtual Client? Client { get; set; }

    [ForeignKey("MoyenPaiementID")]
    [InverseProperty("Factures")]
    public virtual MoyensPaiement? MoyenPaiement { get; set; }

    [InverseProperty("Facture")]
    public virtual ICollection<ElementsFacture> ElementsFactures { get; set; } = new List<ElementsFacture>();

    [InverseProperty("FactureAssociee")]
    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    // Navigation property for BonsLivraison linked to this Facture
    [InverseProperty("Facture")]
    public virtual ICollection<FactureBonLivraison> FactureBonLivraisons { get; set; } = new List<FactureBonLivraison>();
}
