using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class Depense
{
    [Key]
    public int ID { get; set; }

    [StringLength(20)]
    public string? Numero { get; set; }

    public DateOnly? Date { get; set; }

    [StringLength(100)]
    public string? Categorie { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Montant { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    // Audit fields
    [StringLength(50)]
    public string? CreePar { get; set; }

    public DateTime? DateCreation { get; set; }

    [StringLength(50)]
    public string? ModifiePar { get; set; }

    public DateTime? DateModification { get; set; }
}
