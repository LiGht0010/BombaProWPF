using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models
{
    public partial class EmployeBilanCredit
    {
        [Key]
        public int BilanID { get; set; }

        [Required]
        [Display(Name = "Employé")]
        public int EmployeID { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Total Crédit")]
        public decimal TotalCredit { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Total Payé")]
        public decimal TotalPaye { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        [Display(Name = "Balance")]
        public decimal Balance { get; set; }

        // Navigation property
        [ForeignKey("EmployeID")]
        [InverseProperty("EmployeBilanCredit")]
        public virtual Employe? Employe { get; set; }
    }
}
