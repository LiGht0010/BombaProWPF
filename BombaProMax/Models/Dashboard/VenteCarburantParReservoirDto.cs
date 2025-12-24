namespace BombaProMax.Models.Dashboard;

/// <summary>
/// Grouped analytics for Vente Carburant by Reservoir.
/// Shows total quantities sold per reservoir for a specific fuel type.
/// </summary>
public class VenteCarburantParReservoirDto
{
    public int ReservoirId { get; set; }
    public string ReservoirNumero { get; set; } = string.Empty;
    
    // Totals for this reservoir
    public decimal TotalQuantiteElectronique { get; set; }
    public decimal TotalQuantiteMecanique { get; set; }
    public decimal TotalDifference { get; set; }
    public decimal TotalPrix { get; set; }
    
    // Count of periods/readings
    public int NombrePeriodes { get; set; }
}
