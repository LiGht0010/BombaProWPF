using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

public partial class Camion
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    [Display(Name = "Numéro de Véhicule")]
    public string? Matricule { get; set; }

    [StringLength(50)]
    [Display(Name = "Marque")]
    public string? Marque { get; set; }

    public int? CiterneID { get; set; }

    [Required(ErrorMessage = "Le fournisseur est obligatoire")]
    [Display(Name = "Fournisseur")]
    public int? FournisseurID { get; set; }  // Make nullable

    [ForeignKey("FournisseurID")]
    [InverseProperty("Camion")]
    public virtual Fournisseur? Fournisseur { get; set; } = null!;

    [ForeignKey("CiterneID")]
    [InverseProperty("Camion")]
    public virtual Citerne? Citerne { get; set; }

    // Add navigation property for Achats
    [InverseProperty("Camion")]
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();

    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}
