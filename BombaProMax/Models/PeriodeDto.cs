namespace BombaProMax.Models
{
    public class PeriodeDto
    {
        public int PeriodeID { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int? EmployeID { get; set; }
        
        /// <summary>
        /// Payment received via TPE (Terminal de Paiement Électronique / Card payment)
        /// </summary>
        public decimal TPE { get; set; }
        
        /// <summary>
        /// Payment received in cash (Espèces)
        /// </summary>
        public decimal Especes { get; set; }
        
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display field for related entity
        public string? EmployeNom { get; set; }
        
        /// <summary>
        /// Computed total payment (TPE + Especes)
        /// </summary>
        public decimal TotalPaiement => TPE + Especes;
    }
}
