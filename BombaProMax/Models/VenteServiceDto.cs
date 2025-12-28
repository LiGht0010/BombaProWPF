namespace BombaProMax.Models;

/// <summary>
/// Data Transfer Object for VenteService (service sales).
/// </summary>
public class VenteServiceDto
{
    public int ID { get; set; }
    public string? NumeroVente { get; set; }
    public DateTime DateVente { get; set; }
    
    // Service info
    public int ServiceID { get; set; }
    public string? ServiceNumero { get; set; }
    public string? ServiceDescription { get; set; }
    public string? ServiceCategorieNom { get; set; }
    
    public int Quantite { get; set; } = 1;
    public decimal PrixUnitaire { get; set; }
    
    // Computed
    public decimal MontantTotal { get; set; }
    
    // Client info
    public int? ClientID { get; set; }
    public string? ClientNom { get; set; }
    
    // Employee info
    public int? EmployeID { get; set; }
    public string? EmployeNom { get; set; }
    
    // Payment info
    public int? MoyenPaiementID { get; set; }
    public string? MoyenPaiementNom { get; set; }
    
    public string? Notes { get; set; }
    public string? Statut { get; set; }
    
    // Audit
    public DateTime? DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
    public int? CreePar { get; set; }
    public int? ModifiePar { get; set; }
    
    // Display helpers
    public string DateDisplay => DateVente.ToString("dd/MM/yyyy");
    public string MontantDisplay => $"{MontantTotal:N2} DH";
}
