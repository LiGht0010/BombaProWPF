using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models
{
    /// <summary>
    /// Represents detailed meter readings for a specific pump/reservoir during a period
    /// Child entity that stores dual meter system data
    /// </summary>
    public class PeriodeDetails : IValidatableObject
    {
        [Key]
        public int PeriodeDetailID { get; set; }

        [Required]
        [Display(Name = "Période")]
        public int PeriodeID { get; set; }

        [Required]
        [Display(Name = "Pompe")]
        public int? PompeID { get; set; }

        [Required]
        [Display(Name = "Réservoir")]
        public int? ReservoirID { get; set; }

        [Required]
        [Display(Name = "Type de Carburant")]
        public int? ProduitID { get; set; }

        [Required]
        [Display(Name = "Prix du Carburant")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0")]
        public decimal PrixCarburant { get; set; }

        // ? DUAL METER SYSTEM: Electronic meter readings
        [Required]
        [Display(Name = "Compteur Électronique Début")]
        [Range(0, double.MaxValue, ErrorMessage = "Le compteur électronique de début doit être positif")]
        public decimal CompteurElectroniqueDebut { get; set; }

        [Required]
        [Display(Name = "Compteur Électronique Final")]
        [Range(0, double.MaxValue, ErrorMessage = "Le compteur électronique final doit être positif")]
        public decimal CompteurElectroniqueFinal { get; set; }

        // ? DUAL METER SYSTEM: Mechanical meter readings
        [Required]
        [Display(Name = "Compteur Mécanique Début")]
        [Range(0, double.MaxValue, ErrorMessage = "Le compteur mécanique de début doit être positif")]
        public decimal CompteurMecaniqueDebut { get; set; }

        [Required]
        [Display(Name = "Compteur Mécanique Final")]
        [Range(0, double.MaxValue, ErrorMessage = "Le compteur mécanique final doit être positif")]
        public decimal CompteurMecaniqueFinal { get; set; }

        // ? BACKWARD COMPATIBILITY: Keep old properties mapped to new electronic meters
        [NotMapped]
        [Display(Name = "Compteur de Départ")]
        public decimal CompteurDepart
        {
            get => CompteurElectroniqueDebut;
            set => CompteurElectroniqueDebut = value;
        }

        [NotMapped]
        [Display(Name = "Compteur Final")]
        public decimal CompteurFinal
        {
            get => CompteurElectroniqueFinal;
            set => CompteurElectroniqueFinal = value;
        }

        // ? DUAL METER CALCULATIONS: Calculated properties
        [Display(Name = "Quantité Électronique")]
        public decimal QuantiteElectronique => CompteurElectroniqueFinal - CompteurElectroniqueDebut;

        [Display(Name = "Quantité Mécanique")]
        public decimal QuantiteMecanique => CompteurMecaniqueFinal - CompteurMecaniqueDebut;

        [Display(Name = "Différence Quantité (É-M)")]
        public decimal DifferenceQuantite => QuantiteElectronique - QuantiteMecanique;

        [Display(Name = "Prix Total Électronique")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal PrixTotalElectronique => QuantiteElectronique * PrixCarburant;

        [Display(Name = "Prix Total Mécanique")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal PrixTotalMecanique => QuantiteMecanique * PrixCarburant;

        [Display(Name = "Différence Valeur (É-M)")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal DifferenceValeur => DifferenceQuantite * PrixCarburant;

        // ? BACKWARD COMPATIBILITY: Auto-calculated fields (mapped to electronic readings)
        [Display(Name = "Quantité Vendue")]
        public decimal QuantiteVendue => QuantiteElectronique;

        [Display(Name = "Prix Total")]
        [Column(TypeName = "decimal(12, 2)")]
        public decimal PrixTotal => PrixTotalElectronique;

        // Navigation properties
        [ForeignKey("PeriodeID")]
        [InverseProperty("PeriodeDetails")]
        public virtual Periode? Periode { get; set; }

        [ForeignKey("PompeID")]
        public virtual Pompe? Pompe { get; set; }

        [ForeignKey("ReservoirID")]
        public virtual Reservoir? Reservoir { get; set; }

        [ForeignKey("ProduitID")]
        public virtual Produit? Produit { get; set; }

        /// <summary>
        /// Stock lot consumption records for this detail (FIFO audit trail)
        /// </summary>
        [InverseProperty("PeriodeDetail")]
        public virtual ICollection<StockLotConsumption> StockLotConsumptions { get; set; } = new List<StockLotConsumption>();

        // ➡️ ENHANCED VALIDATION: Updated validation for dual meter system
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Electronic meter validation
            if (CompteurElectroniqueFinal <= CompteurElectroniqueDebut)
            {
                yield return new ValidationResult(
                    "Le compteur électronique final doit être supérieur au compteur électronique de début",
                    new[] { nameof(CompteurElectroniqueFinal) });
            }

            // Mechanical meter validation
            if (CompteurMecaniqueFinal <= CompteurMecaniqueDebut)
            {
                yield return new ValidationResult(
                    "Le compteur mécanique final doit être supérieur au compteur mécanique de début",
                    new[] { nameof(CompteurMecaniqueFinal) });
            }

            // ? BUSINESS LOGIC: Warning for large differences between electronic and mechanical meters
            var differenceAbsolue = Math.Abs(DifferenceQuantite);
            var quantiteMax = Math.Max(QuantiteElectronique, QuantiteMecanique);

            if (quantiteMax > 0)
            {
                var pourcentageDifference = (differenceAbsolue / quantiteMax) * 100;
                if (pourcentageDifference > 5) // 5% threshold
                {
                    yield return new ValidationResult(
                        $"Attention: Différence importante entre compteurs ({differenceAbsolue:F3}L - {pourcentageDifference:F1}%). Veuillez vérifier les relevés.",
                        new[] { nameof(CompteurElectroniqueFinal), nameof(CompteurMecaniqueFinal) });
                }
            }
        }
    }
}