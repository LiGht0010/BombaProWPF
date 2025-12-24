using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombaProMaxApi.Models;

    public partial class Reservoir
    {
        [Key]
        public int ID { get; set; }

        [StringLength(20)]
        public string Numero { get; set; } = null!;

        // Change from TypeDeCarburantID to ProduitID
        [Display(Name = "Type de Carburant")]
        public int? ProduitID { get; set; }  // Now points to a Product (fuel type)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Capacite { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal NiveauDeCarburant { get; set; }

        /// <summary>
        /// Maximum height in cm for this reservoir (from calibration table)
        /// </summary>
        [Column(TypeName = "decimal(6, 2)")]
        [Display(Name = "Hauteur Maximale (cm)")]
        public decimal? HauteurMax { get; set; }

        // ???????????????????????????????????????????????????????????????
        // MANUFACTURER / CALIBRATION CERTIFICATE INFO
        // ???????????????????????????????????????????????????????????????

        /// <summary>
        /// Tank manufacturer name (e.g., "PETROTANK S.A.")
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Fabricant")]
        public string? Fabricant { get; set; }

        /// <summary>
        /// Tank serial number from manufacturer (e.g., "30 1054")
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Numéro de Série")]
        public string? NumeroSerie { get; set; }

        /// <summary>
        /// Tank diameter in millimeters (e.g., 2500 for 2500mm)
        /// </summary>
        [Column(TypeName = "decimal(8, 2)")]
        [Display(Name = "Diamètre (mm)")]
        public decimal? DiametreMm { get; set; }

        // Navigation property
        [ForeignKey("ProduitID")]
        [InverseProperty("Reservoirs")]
        public virtual Produit? Produit { get; set; }

        [InverseProperty("ReservoirAssocie")]
        public virtual ICollection<Pompe> Pompes { get; set; } = new List<Pompe>();

        [InverseProperty("Reservoir")]
        public virtual ICollection<AchatAllocation> AchatAllocations { get; set; } = new List<AchatAllocation>();

        [InverseProperty("Reservoir")]
        public virtual ICollection<JaugeageDetail> JaugeageDetails { get; set; } = new List<JaugeageDetail>();

        /// <summary>
        /// Stock lots currently in this reservoir (for FIFO inventory tracking)
        /// </summary>
        [InverseProperty("Reservoir")]
        public virtual ICollection<StockLot> StockLots { get; set; } = new List<StockLot>();

        /// <summary>
        /// Calibration table entries for this reservoir (height to volume mapping)
        /// </summary>
        [InverseProperty("Reservoir")]
        public virtual ICollection<ReservoirCalibration> Calibrations { get; set; } = new List<ReservoirCalibration>();

        [Display(Name = "Ajouté Par")]
        public int? AjoutePar { get; set; }

        [Display(Name = "Date de Creation")]
        public DateTime? DateCreation { get; set; }

        [Display(Name = "Modifié Par")]
        public int? ModifiePar { get; set; }

        [Display(Name = "Date de Modification")]
        public DateTime? DateModification { get; set; }
    }
