using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BombaProMaxApi.Models;

public partial class IndicateursFinancier
{
    [Key]
    public int ID { get; set; }

    public DateOnly? DebutPeriode { get; set; }

    public DateOnly? FinPeriode { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? ChiffreAffairesTotal { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? DepensesTotales { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? BeneficeNet { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalAchats { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalVentes { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalCarburantVendu { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalProduitsVendus { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? TotalServicesVendus { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? DateCreation { get; set; }
}
