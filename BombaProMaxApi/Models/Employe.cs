using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;


namespace BombaProMaxApi.Models
{
    public partial class Employe
    {
        [Key]
        public int ID { get; set; }

        [StringLength(50)]
        public string Nom { get; set; } = null!;

        [StringLength(50)]
        public string Prenom { get; set; } = null!;
    
        [StringLength(20)]
        public string CIN { get; set; }

        [StringLength(15)]
        public string? Telephone { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(100)]
        public string? Poste { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Salaire { get; set; }

        // Navigation property for Jaugeages
        [InverseProperty("Temoin")]
        public virtual ICollection<Jaugeage> Jaugeages { get; set; } = new List<Jaugeage>();

        // Navigation property for EmployeBilanCredit (one-to-one relationship)
        [InverseProperty("Employe")]
        public virtual EmployeBilanCredit? EmployeBilanCredit { get; set; }

        // Navigation property for EmployeCreditTransactions (one-to-many relationship)
        [InverseProperty("Employe")]
        public virtual ICollection<EmployeCreditTransaction> EmployeCreditTransactions { get; set; } = new List<EmployeCreditTransaction>();

        // Navigation property for EmployeReglementsCredit (one-to-many relationship)
        [InverseProperty("Employe")]
        public virtual ICollection<EmployeReglementCredit> EmployeReglementsCredit { get; set; } = new List<EmployeReglementCredit>();

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
