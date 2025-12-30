using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

/// <summary>
/// Represents a cash deposit from the station to the bank account.
/// Tracks when cash (Espèces) is deposited to reduce station cash holdings.
/// </summary>
[Table("DepotsCaisse")]
public class DepotCaisse
{
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// Amount deposited to the bank.
    /// </summary>
    [Required(ErrorMessage = "Le montant est obligatoire")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 99999999.99, ErrorMessage = "Le montant doit être entre 0.01 et 99,999,999.99")]
    [Display(Name = "Montant Déposé")]
    public decimal Montant { get; set; }

    /// <summary>
    /// Date and time of the deposit.
    /// </summary>
    [Required(ErrorMessage = "La date de dépôt est obligatoire")]
    [Display(Name = "Date du Dépôt")]
    public DateTime DateDepot { get; set; }

    /// <summary>
    /// Bank reference or receipt number.
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Référence Bancaire")]
    public string? ReferenceBancaire { get; set; }

    /// <summary>
    /// Name of the bank or branch.
    /// </summary>
    [StringLength(200)]
    [Display(Name = "Banque")]
    public string? Banque { get; set; }

    /// <summary>
    /// Additional notes about the deposit.
    /// </summary>
    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Confirmation document file name.
    /// </summary>
    [StringLength(255)]
    [Display(Name = "Nom du Justificatif")]
    public string? PieceJustificativeNom { get; set; }

    /// <summary>
    /// Confirmation document stored as Base64 string.
    /// </summary>
    [Display(Name = "Piece Justificative")]
    public string? PieceJustificativeBase64 { get; set; }

    /// <summary>
    /// MIME type of the confirmation document.
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Type du Justificatif")]
    public string? PieceJustificativeType { get; set; }

    /// <summary>
    /// User who validated/created this deposit.
    /// </summary>
    [Display(Name = "Validé Par")]
    public int? ValidePar { get; set; }

    // Audit fields
    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Création")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }

    // Navigation properties
    [ForeignKey("ValidePar")]
    public virtual User? Validateur { get; set; }
}
