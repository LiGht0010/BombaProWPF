using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class BilanCredit
{
    [Key]
    public int BilanID { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientID { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    [Display(Name = "Total Crédit")]
    public decimal TotalCredit { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    [Display(Name = "Total Payé")]
    public decimal TotalPaye { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    [Display(Name = "Balance")]
    public decimal Balance { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    [Display(Name = "Crédit Facturé")]
    public decimal CreditFacture { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    [Display(Name = "Crédit Non Facturé")]
    public decimal CreditNonFacture { get; set; }

    [Display(Name = "Dernière Mise à Jour")]
    public DateTime DerniereMiseAJour { get; set; }

    [Display(Name = "Période Début")]
    public DateTime? PeriodeDebut { get; set; }

    [Display(Name = "Période Fin")]
    public DateTime? PeriodeFin { get; set; }

    // Navigation property
    [ForeignKey("ClientID")]
    [InverseProperty("BilanCredit")]
    public virtual Client? Client { get; set; }
}
