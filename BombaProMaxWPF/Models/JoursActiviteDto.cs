namespace BombaProMaxWPF.Models
{
    public class JoursActiviteDto
    {
        public int ID { get; set; }
        public DateOnly Date { get; set; }
        public decimal? ChiffreAffaires { get; set; }
        public decimal? FraisExploitation { get; set; }
    }
}
