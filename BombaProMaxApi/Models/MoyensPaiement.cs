using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

[Table("MoyensPaiement")]
public partial class MoyensPaiement
{
    [Key]
    public int ID { get; set; }

    [StringLength(50)]
    public string? Nom { get; set; }

    [InverseProperty("MoyenPaiement")]
    public virtual ICollection<Facture> Factures { get; set; } = new List<Facture>();

    [InverseProperty("ModePaiement")]
    public virtual ICollection<ReglementCredit> ReglementsCredit { get; set; } = new List<ReglementCredit>();

    // Navigation property for EmployeReglementsCredit
    [InverseProperty("ModePaiement")]
    public virtual ICollection<EmployeReglementCredit> EmployeReglementsCredit { get; set; } = new List<EmployeReglementCredit>();
}
