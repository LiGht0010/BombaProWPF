namespace BombaProMaxWPF.Models
{
    public class JaugeageDetailDto
    {
        public int ID { get; set; }
        public int JaugeageID { get; set; }
        public int ReservoirID { get; set; }
        public decimal HauteurMesuree { get; set; }
        public decimal VolumeCalcule { get; set; }
        public decimal? Temperature { get; set; }
        public string? Notes { get; set; }

        // Display fields for related entities
        public string? JaugeageNumero { get; set; }
        public string? ReservoirNumero { get; set; }
    }
}
