using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models
{
    public partial class EmployeCreditTransaction
    {
        [Key]
        public int CreditID { get; set; }

        [StringLength(20)]
        [Display(Name = "Numéro Transaction")]
        public string? NumeroTransaction { get; set; }

        [Required]
        [Display(Name = "Employé")]
        public int EmployeID { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Montant Total")]
        public decimal MontantTotal { get; set; }

        [Required]
        [Display(Name = "Date Crédit")]
        public DateTime DateCredit { get; set; }

        [Display(Name = "Ajouté Par")]
        public int? AjoutePar { get; set; }

        [Display(Name = "Date de Creation")]
        public DateTime? DateCreation { get; set; }

        [Display(Name = "Modifié Par")]
        public int? ModifiePar { get; set; }

        [Display(Name = "Date de Modification")]
        public DateTime? DateModification { get; set; }

        [ForeignKey("EmployeID")]
        [InverseProperty("EmployeCreditTransactions")]
        public virtual Employe? Employe { get; set; }
    }

}
