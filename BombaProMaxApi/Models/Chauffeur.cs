using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Chauffeur
{
    [Key]
    public int ID { get; set; }

    [StringLength(50)]
    public string Nom { get; set; }

    [StringLength(50)]
    public string? Prenom { get; set; }

    [StringLength(20)]
    public string? CIN { get; set; }

    [StringLength(20)]
    public string? Telephone { get; set; }

    [StringLength(50)]
    public string? NumeroPermis { get; set; }

    public int? FournisseurID { get; set; }

    [InverseProperty("Chauffeur")]
    public virtual ICollection<Achat> Achats { get; set; } = new List<Achat>();


    [ForeignKey("FournisseurID")]
    [InverseProperty("Chauffeurs")]
    public virtual Fournisseur? Fournisseur { get; set; }

    [Display(Name = "Ajouté Par")]
    public int? AjoutePar { get; set; }

    [Display(Name = "Date de Creation")]
    public DateTime? DateCreation { get; set; }

    [Display(Name = "Modifié Par")]
    public int? ModifiePar { get; set; }

    [Display(Name = "Date de Modification")]
    public DateTime? DateModification { get; set; }
}

