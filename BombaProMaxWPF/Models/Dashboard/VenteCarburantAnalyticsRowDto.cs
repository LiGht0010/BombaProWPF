namespace BombaProMaxWPF.Models.Dashboard;

/// <summary>
/// Raw analytics row for Vente Carburant (fuel sales from Periode/PeriodeDetails).
/// Contains pump meter readings with dual meter system data.
/// </summary>
public class VenteCarburantAnalyticsRowDto
{
    // Periode info
    public int PeriodeId { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    
    // Product/Fuel info
    public int ProduitId { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    
    // Pump info
    public int PompeId { get; set; }
    public string PompeNumero { get; set; } = string.Empty;
    
    // Reservoir info
    public int ReservoirId { get; set; }
    public string ReservoirNumero { get; set; } = string.Empty;
    
    // Price
    public decimal PrixCarburant { get; set; }
    
    // Electronic meter readings
    public decimal CompteurElectroniqueDebut { get; set; }
    public decimal CompteurElectroniqueFinal { get; set; }
    public decimal QuantiteElectronique { get; set; }
    public decimal PrixTotalElectronique { get; set; }
    
    // Mechanical meter readings
    public decimal CompteurMecaniqueDebut { get; set; }
    public decimal CompteurMecaniqueFinal { get; set; }
    public decimal QuantiteMecanique { get; set; }
    public decimal PrixTotalMecanique { get; set; }
    
    // Difference between meters
    public decimal DifferenceQuantite { get; set; }
    public decimal DifferenceValeur { get; set; }
    
    // Employee info
    public string? EmployeNom { get; set; }
}
