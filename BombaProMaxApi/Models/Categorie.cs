using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Categorie
{
    [Key]
    public int ID { get; set; }

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
    [StringLength(100)]
    [Display(Name = "Nom de Catégorie")]
    public string Nom { get; set; } = null!;

    // Navigation property for related products
    [InverseProperty("Categorie")]
    public virtual ICollection<Produit> Produits { get; set; } = new List<Produit>();
}
