using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class ReglementCredit
{
    [Key]
    public int ReglementID { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientID { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Montant Payé")]
    public decimal MontantPaye { get; set; }

    [Required]
    [Display(Name = "Mode de Paiement")]
    public int ModePaiementID { get; set; }

    [StringLength(50)]
    [Display(Name = "Référence Transaction")]
    public string? ReferenceTransaction { get; set; }

    [StringLength(100)]
    [Display(Name = "Validé Par")]
    public string? ValidePar { get; set; }

    [Required]
    [Display(Name = "Date Règlement")]
    public DateTime DateReglement { get; set; }

    [StringLength(500)]
    [Display(Name = "Remarques")]
    public string? Remarques { get; set; }

    // Navigation properties
    [ForeignKey("ClientID")]
    [InverseProperty("ReglementsCredit")]
    public virtual Client? Client { get; set; }

    [ForeignKey("ModePaiementID")]
    [InverseProperty("ReglementsCredit")]
    public virtual MoyensPaiement? ModePaiement { get; set; }



    


    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}
