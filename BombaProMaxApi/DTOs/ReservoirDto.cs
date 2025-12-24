namespace BombaProMaxApi.DTOs
{
    public class ReservoirDto
    {
        public int ID { get; set; }
        public string Numero { get; set; } = null!;
        public int? ProduitID { get; set; }
        public decimal Capacite { get; set; }
        public decimal NiveauDeCarburant { get; set; }
        public decimal? HauteurMax { get; set; }

        // Manufacturer / Calibration Certificate Info
        public string? Fabricant { get; set; }
        public string? NumeroSerie { get; set; }
        public decimal? DiametreMm { get; set; }

        // Audit fields
        public int? AjoutePar { get; set; }
        public DateTime? DateCreation { get; set; }
        public int? ModifiePar { get; set; }
        public DateTime? DateModification { get; set; }

        // Display field for related entity
        public string? ProduitNom { get; set; }

        // Indicates if calibration data is available for this reservoir
        public bool HasCalibration { get; set; }
        public int CalibrationCount { get; set; }
    }
}
