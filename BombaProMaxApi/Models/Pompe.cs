using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;


namespace BombaProMaxApi.Models;

public partial class Pompe
{
    [Key]
    public int ID { get; set; }

    [Required(ErrorMessage = "Le numéro de la pompe est requis.")]
    [StringLength(20, ErrorMessage = "Le numéro ne peut pas dépasser 20 caractères.")]
    [Display(Name = "Numéro de la Pompe")]
    public string Numero { get; set; } = null;

    [Required(ErrorMessage = "Le statut de la pompe est requis.")]
    [StringLength(50, ErrorMessage = "Le statut ne peut pas dépasser 50 caractères.")]
    [Display(Name = "Statut")]
    public string Statut { get; set; } = null;

    // ? DUAL METER SYSTEM: Electronic meter (renamed from existing Compteur)
    [Column(TypeName = "decimal(18, 3)")]
    [Display(Name = "Compteur Électronique Actuel (L)")]
    public decimal? CompteurElectroniqueActuel { get; set; }

    // ? DUAL METER SYSTEM: New mechanical meter
    [Column(TypeName = "decimal(18, 3)")]
    [Display(Name = "Compteur Mécanique Actuel (L)")]
    public decimal? CompteurMecaniqueActuel { get; set; }


    [Display(Name = "Réservoir Associé")]
    public int? ReservoirAssocieID { get; set; }  // Make nullable


    [ForeignKey("ReservoirAssocieID")]
    [InverseProperty("Pompes")]
    [JsonIgnore]
    public virtual Reservoir ReservoirAssocie { get; set; } = null;



    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}