using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models
{
    /// <summary>
    /// Represents a business period/shift for tracking pump operations
    /// Parent entity that contains multiple PeriodeDetails for each pump/reservoir
    /// </summary>
    public class Periode : IValidatableObject
    {
        [Key]
        public int PeriodeID { get; set; }

        [Required]
        [Display(Name = "Date/Heure de Début")]
        public DateTime DateDebut { get; set; }

        [Required]
        [Display(Name = "Date/Heure de Fin")]
        public DateTime DateFin { get; set; }

        [Display(Name = "Employé Responsable")]
        public int? EmployeID { get; set; }

        /// <summary>
        /// Payment received via TPE (Terminal de Paiement Électronique / Card payment)
        /// </summary>
        [Display(Name = "Paiement TPE")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TPE { get; set; }

        /// <summary>
        /// Payment received in cash (Espèces)
        /// </summary>
        [Display(Name = "Paiement Espèces")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Especes { get; set; }

        // Audit fields
        [Display(Name = "Ajouté Par")]
        public int? AjoutePar { get; set; }

        [Display(Name = "Date de Creation")]
        public DateTime? DateCreation { get; set; }

        [Display(Name = "Modifié Par")]
        public int? ModifiePar { get; set; }

        [Display(Name = "Date de Modification")]
        public DateTime? DateModification { get; set; }

        // Navigation properties
        [ForeignKey("EmployeID")]
        public virtual Employe? Employe { get; set; }

        [InverseProperty("Periode")]
        public virtual ICollection<PeriodeDetails> PeriodeDetails { get; set; } = new List<PeriodeDetails>();

        // Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Date validation
            if (DateFin <= DateDebut)
            {
                yield return new ValidationResult(
                    "La date de fin doit être postérieure à la date de début",
                    new[] { nameof(DateFin) });
            }

            // TPE cannot be negative
            if (TPE < 0)
            {
                yield return new ValidationResult(
                    "Le montant TPE ne peut pas être négatif",
                    new[] { nameof(TPE) });
            }

            // Especes cannot be negative
            if (Especes < 0)
            {
                yield return new ValidationResult(
                    "Le montant Espèces ne peut pas être négatif",
                    new[] { nameof(Especes) });
            }
        }
    }
}