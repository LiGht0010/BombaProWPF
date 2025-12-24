using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

public partial class Client
{
    [Key]
    public int ID { get; set; }

    [Required]
    [StringLength(20)]
    public string NumeroClient { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Nom { get; set; } = null!;

    [StringLength(100)]
    public string? Contact { get; set; }

    [Required]
    [StringLength(100)]
    public string CIN { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string NomSociete { get; set; } = null!;

    // The user who created or modified the client
    public int userID { get; set; }

    // Audit fields
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }

    // Navigation properties
    
    [InverseProperty("Client")]
    public virtual ICollection<Facture> Factures { get; set; } = new List<Facture>();

    [InverseProperty("Client")]
    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    [InverseProperty("Client")]
    public virtual ICollection<ReglementCredit> ReglementsCredit { get; set; } = new List<ReglementCredit>();

    [InverseProperty("Client")]
    public virtual BilanCredit? BilanCredit { get; set; }

    // Navigation property for BonsLivraison
    [InverseProperty("Client")]
    public virtual ICollection<BonLivraison> BonsLivraison { get; set; } = new List<BonLivraison>();
}
