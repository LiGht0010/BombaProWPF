using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models
{
    public class Citerne
    {
        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        [Display(Name = "Matricule Citerne")]
        public string? MatriculeCiterne { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Capacité")]
        [Range(0, double.MaxValue, ErrorMessage = "La capacité doit être positive")]
        public decimal? Capacite { get; set; }

        //nombre de partitions
        [Display(Name = "Nombre de Partitions")]
        public uint? PartitionsNumber { get; set; }
        

        [Required(ErrorMessage = "Le fournisseur est obligatoire")]
        [Display(Name = "Fournisseur")]
        public int? FournisseurID { get; set; }  // Make nullable

        [ForeignKey("FournisseurID")]
        [InverseProperty("Citerne")]
        public virtual Fournisseur? Fournisseur { get; set; } = null!;

        // Navigation property for Camion (one-to-many: one Citerne can be in many Camions)
        [InverseProperty("Citerne")]
        public virtual ICollection<Camion> Camion { get; set; } = new List<Camion>();

        [Display(Name = "Ajouté Par")]
        public int? AjoutePar { get; set; }

        [Display(Name = "Date de Creation")]
        public DateTime? DateCreation { get; set; }

        [Display(Name = "Modifié Par")]
        public int? ModifiePar { get; set; }

        [Display(Name = "Date de Modification")]
        public DateTime? DateModification { get; set; }

    }
}
