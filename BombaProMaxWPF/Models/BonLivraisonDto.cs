using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BombaProMaxWPF.Models
{
    public partial class BonLivraisonDto : ObservableObject
    {
        public int ID { get; set; }
        public string NumeroBL { get; set; } = null!;
        public DateOnly DateBL { get; set; }
        public int ClientID { get; set; }
        public decimal MontantTotal { get; set; }
        public bool EstFacture { get; set; }
        public string? Notes { get; set; }

        // Audit fields
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display fields
        public string? ClientNom { get; set; }
        public string? ClientNumero { get; set; }

        // Details collection
        public List<BonLivraisonDetailsDto>? Details { get; set; }

        // UI selection (observable for binding)
        [ObservableProperty]
        private bool _isSelected;
    }

    public class CreateBonLivraisonDto
    {
        public string NumeroBL { get; set; } = null!;
        public DateOnly DateBL { get; set; }
        public int ClientID { get; set; }
        public string? Notes { get; set; }
        public int? AjoutePar { get; set; }
        public List<CreateBonLivraisonDetailsDto> Details { get; set; } = new();
    }

    public class UpdateBonLivraisonDto
    {
        public int ID { get; set; }
        public string NumeroBL { get; set; } = null!;
        public DateOnly DateBL { get; set; }
        public int ClientID { get; set; }
        public string? Notes { get; set; }
        public int? ModifiePar { get; set; }
        public List<CreateBonLivraisonDetailsDto> Details { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for merging multiple BLs into one
    /// </summary>
    public class MergeBLsDto
    {
        public List<int> BonLivraisonIds { get; set; } = [];
        public int ClientID { get; set; }
        public DateOnly? DateBL { get; set; }
        public string? Notes { get; set; }
        public int? CreatedByUserId { get; set; }
    }

    /// <summary>
    /// Result DTO for BL merge operations
    /// </summary>
    public class MergeBLsResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? NewBonLivraisonID { get; set; }
        public string? NewNumeroBL { get; set; }
        public decimal MontantTotal { get; set; }
        public int BLsMerged { get; set; }
        public List<string>? Errors { get; set; }
    }
}
