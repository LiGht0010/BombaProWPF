using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Fournisseur
{
    [Key]
    public int ID { get; set; }

    [StringLength(50)]
    [Display(Name = "Prénom")]
    public string? Prenom { get; set; }

    [StringLength(50)]
    [Display(Name = "Nom")]
    public string? Nom { get; set; }

    [StringLength(50)]
    [Display(Name = "Société")]
    public string? Societe { get; set; }

    [StringLength(200)]
    [Display(Name = "Adresse")]
    public string? Adresse { get; set; }

    [StringLength(20)]
    [Display(Name = "Téléphone")]
    public string? Telephone { get; set; }

    [StringLength(100)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(30)]
    [Display(Name = "Numéro de Compte Bancaire")]
    public string? RIB { get; set; }

    [StringLength(50)]
    [Display(Name = "Contact")]
    public string? Contact { get; set; }

    [StringLength(50)]
    [Display(Name = "Conditions de Paiement")]
    public string? ConditionsPaiement { get; set; }

    [StringLength(50)]
    public string Statut { get; set; } = "Actif";

    // Audit fields
    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }

    // Navigation property for related Achats
    [InverseProperty("Fournisseur")]
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();

    // Navigation property for related Camions
    [InverseProperty("Fournisseur")]
    public virtual ICollection<Camion> Camion { get; set; } = new List<Camion>();

    // Navigation property for related Citernes
    [InverseProperty("Fournisseur")]
    public virtual ICollection<Citerne> Citerne { get; set; } = new List<Citerne>();

    // Navigation property for related Chauffeurs
    [InverseProperty("Fournisseur")]
    public virtual ICollection<Chauffeur> Chauffeurs { get; set; } = new List<Chauffeur>();
}
