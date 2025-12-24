using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Suivi
namespace BombaProMaxApi.Models
{
    public partial class Jaugeage
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Date du Jaugeage")]
        public DateTime DateJaugeage { get; set; }

        [Required]
        [Display(Name = "Témoin")]
        public int TemoinID { get; set; }

        [StringLength(50)]
        [Display(Name = "Numéro de Jaugeage")]
        public string? NumeroJaugeage { get; set; }

        [StringLength(500)]
        [Display(Name = "Observations")]
        public string? Observations { get; set; }

        // Navigation properties
        [ForeignKey("TemoinID")]
        [InverseProperty("Jaugeages")]
        public virtual Employe? Temoin { get; set; }

        // Navigation property for JaugeageDetails
        [InverseProperty("Jaugeage")]
        public virtual ICollection<JaugeageDetail> JaugeageDetails { get; set; } = new List<JaugeageDetail>();

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
