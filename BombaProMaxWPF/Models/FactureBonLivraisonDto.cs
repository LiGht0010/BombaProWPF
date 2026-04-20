using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombaProMaxWPF.Models
{
    public class FactureBonLivraisonDto
    {
        public int ID { get; set; }
        public int FactureID { get; set; }
        public int BonLivraisonID { get; set; }
        public DateTime DateAssociation { get; set; }

        // Display fields
        public string? FactureNumero { get; set; }
        public string? BonLivraisonNumero { get; set; }
        public DateOnly? BonLivraisonDate { get; set; }
        public decimal? BonLivraisonMontant { get; set; }
    }

    /// <summary>
    /// Request DTO for creating a Facture from multiple BonLivraisons
    /// </summary>
    public class CreateFactureFromBLsDto
    {
        public List<int> BonLivraisonIds { get; set; } = [];
        public int ClientID { get; set; }
        public DateOnly? DateFacture { get; set; }
        public int? MoyenPaiementID { get; set; }
        public string? Notes { get; set; }
        public int? CreatedByUserId { get; set; }
    }

    /// <summary>
    /// Result DTO for BL to Facture conversion
    /// </summary>
    public class FacturationResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? FactureID { get; set; }
        public string? NumeroFacture { get; set; }
        public decimal? MontantTotal { get; set; }
        public int BLsFactures { get; set; }
        public List<string>? Errors { get; set; }
    }
}
