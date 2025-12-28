using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Represents a service sale transaction.
/// </summary>
[Table("VenteServices")]
public partial class VenteService
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    [Display(Name = "Numéro de Vente")]
    public string? NumeroVente { get; set; }

    [Required(ErrorMessage = "La date de vente est obligatoire")]
    [Display(Name = "Date de Vente")]
    public DateTime DateVente { get; set; }

    [Required(ErrorMessage = "Le service est obligatoire")]
    [Display(Name = "Service")]
    public int ServiceID { get; set; }

    [Required(ErrorMessage = "La quantité est obligatoire")]
    [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0")]
    [Display(Name = "Quantité")]
    public int Quantite { get; set; } = 1;

    [Required(ErrorMessage = "Le prix unitaire est obligatoire")]
    [Column(TypeName = "decimal(10, 2)")]
    [Range(0.01, 99999999.99, ErrorMessage = "Le prix unitaire doit être entre 0.01 et 99,999,999.99")]
    [Display(Name = "Prix Unitaire")]
    public decimal PrixUnitaire { get; set; }

    [Display(Name = "Client")]
    public int? ClientID { get; set; }

    [Display(Name = "Employé")]
    public int? EmployeID { get; set; }

    [Display(Name = "Mode de Paiement")]
    public int? MoyenPaiementID { get; set; }

    [StringLength(500)]
    [Display(Name = "Notes/Observations")]
    public string? Notes { get; set; }

    [StringLength(50)]
    [Display(Name = "Statut de la Vente")]
    public string? Statut { get; set; } = "Confirmée";

    // Computed properties
    [NotMapped]
    [Display(Name = "Montant Total")]
    public decimal MontantTotal => PrixUnitaire * Quantite;

    // Navigation properties
    [ForeignKey("ServiceID")]
    public virtual Service Service { get; set; } = null!;

    [ForeignKey("ClientID")]
    public virtual Client? Client { get; set; }

    [ForeignKey("EmployeID")]
    public virtual Employe? Employe { get; set; }

    [ForeignKey("MoyenPaiementID")]
    public virtual MoyensPaiement? MoyenPaiement { get; set; }

    // Audit fields
    [Display(Name = "Date de Création")]
    public DateTime? DateCreation { get; set; } = DateTime.UtcNow;

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }

    [Display(Name = "Créé Par")]
    public int? CreePar { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    /// <summary>
    /// Generates a unique sale number.
    /// </summary>
    public string GenerateNumeroVente()
    {
        var date = DateVente.ToString("yyyyMMdd");
        var time = DateVente.ToString("HHmmss");
        return $"VS-{date}-{time}";
    }
}
