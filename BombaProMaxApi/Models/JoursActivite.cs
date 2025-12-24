using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

[Table("JoursActivité")]
public partial class JoursActivite
{
    [Key]
    public int ID { get; set; }

    public DateOnly Date { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? ChiffreAffaires { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? FraisExploitation { get; set; }
}
