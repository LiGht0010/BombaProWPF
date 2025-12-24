using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Suivi
namespace BombaProMaxApi.Models
{
    public partial class JaugeageDetail
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int JaugeageID { get; set; }

        [Required]
        [Display(Name = "Réservoir")]
        public int ReservoirID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Hauteur Mesurée (cm)")]
        public decimal HauteurMesuree { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Volume Calculé (L)")]
        public decimal VolumeCalcule { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Température (°C)")]
        public decimal? Temperature { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("JaugeageID")]
        [InverseProperty("JaugeageDetails")]
        public virtual Jaugeage Jaugeage { get; set; } = null!;

        [ForeignKey("ReservoirID")]
        [InverseProperty("JaugeageDetails")]
        public virtual Reservoir Reservoir { get; set; } = null!;
    }
}
