using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns raw Achat analytics rows for the specified period.
    /// Supports multiple filter modes: date range, specific date, year, or month.
    /// </summary>
    /// <param name="startDate">Start date for range filter (inclusive)</param>
    /// <param name="endDate">End date for range filter (inclusive)</param>
    /// <param name="date">Specific date filter</param>
    /// <param name="year">Year filter (e.g., 2025)</param>
    /// <param name="month">Month filter in yyyy-MM format (e.g., 2025-03)</param>
    [HttpGet("achats")]
    public async Task<ActionResult<IEnumerable<AchatAnalyticsRowDto>>> GetAchatsAnalytics(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] int? year = null,
        [FromQuery] string? month = null)
    {
        var query = _context.Achats
            .Include(a => a.Produit)
                .ThenInclude(p => p!.Categorie)
            .Where(a => a.ProduitID != null && a.Quantite != null)
            .AsNoTracking();

        // Apply filters (priority: specific date > date range > month > year)
        if (date.HasValue)
        {
            query = query.Where(a => a.Date == date.Value);
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            query = query.Where(a => a.Date >= startDate.Value && a.Date <= endDate.Value);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            query = query.Where(a => a.Date >= monthStart && a.Date <= monthEnd);
        }
        else if (year.HasValue)
        {
            var yearStart = new DateOnly(year.Value, 1, 1);
            var yearEnd = new DateOnly(year.Value, 12, 31);
            query = query.Where(a => a.Date >= yearStart && a.Date <= yearEnd);
        }

        var results = await query
            .OrderByDescending(a => a.Date)
            .Select(a => new AchatAnalyticsRowDto
            {
                ProduitId = a.ProduitID!.Value,
                ProduitNom = a.Produit!.Description ?? a.Produit.NumeroProduit,
                CategorieNom = a.Produit.Categorie != null ? a.Produit.Categorie.Nom : null,
                Quantite = a.Quantite!.Value,
                PrixAchat = a.PrixAchatUnitaire ?? 0,
                PrixTotal = a.Cout ?? (a.Quantite!.Value * (a.PrixAchatUnitaire ?? 0)),
                DateAchat = a.Date
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Returns raw Vente (Lubrifiants et Articles) analytics rows for the specified period.
    /// Supports multiple filter modes: date range, specific date, year, or month.
    /// </summary>
    /// <param name="startDate">Start date for range filter (inclusive)</param>
    /// <param name="endDate">End date for range filter (inclusive)</param>
    /// <param name="date">Specific date filter</param>
    /// <param name="year">Year filter (e.g., 2025)</param>
    /// <param name="month">Month filter in yyyy-MM format (e.g., 2025-03)</param>
    [HttpGet("ventes")]
    public async Task<ActionResult<IEnumerable<VenteAnalyticsRowDto>>> GetVentesAnalytics(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] int? year = null,
        [FromQuery] string? month = null)
    {
        var query = _context.VenteLubrifiantsEtArticles
            .Include(v => v.Produit)
                .ThenInclude(p => p!.Categorie)
            .Include(v => v.Client)
            .AsNoTracking();

        // Apply filters (priority: specific date > date range > month > year)
        if (date.HasValue)
        {
            var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
            var nextDay = dateTime.AddDays(1);
            query = query.Where(v => v.DateVente >= dateTime && v.DateVente < nextDay);
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(v => v.DateVente >= startDateTime && v.DateVente <= endDateTime);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            var startDateTime = monthStart.ToDateTime(TimeOnly.MinValue);
            var endDateTime = monthEnd.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(v => v.DateVente >= startDateTime && v.DateVente <= endDateTime);
        }
        else if (year.HasValue)
        {
            var yearStart = new DateTime(year.Value, 1, 1);
            var yearEnd = new DateTime(year.Value, 12, 31, 23, 59, 59);
            query = query.Where(v => v.DateVente >= yearStart && v.DateVente <= yearEnd);
        }

        var results = await query
            .OrderByDescending(v => v.DateVente)
            .Select(v => new VenteAnalyticsRowDto
            {
                ProduitId = v.ProduitID,
                ProduitNom = v.Produit!.Description ?? v.Produit.NumeroProduit,
                CategorieNom = v.Produit.Categorie != null ? v.Produit.Categorie.Nom : null,
                Quantite = v.QuantiteVendue,
                PrixVente = v.PrixUnitaireTTC,
                PrixTotal = v.PrixUnitaireTTC * v.QuantiteVendue,
                DateVente = DateOnly.FromDateTime(v.DateVente),
                ClientNom = v.Client != null ? v.Client.Nom : null
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Returns raw Vente Carburant analytics rows (from Periode/PeriodeDetails) for the specified period.
    /// Contains pump meter readings with dual meter system data.
    /// </summary>
    [HttpGet("ventes-carburant")]
    public async Task<ActionResult<IEnumerable<VenteCarburantAnalyticsRowDto>>> GetVentesCarburantAnalytics(
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        [FromQuery] DateOnly? date = null,
        [FromQuery] int? year = null,
        [FromQuery] string? month = null)
    {
        var query = _context.PeriodeDetails
            .Include(pd => pd.Periode)
                .ThenInclude(p => p!.Employe)
            .Include(pd => pd.Produit)
            .Include(pd => pd.Pompe)
            .Include(pd => pd.Reservoir)
            .Where(pd => pd.ProduitID != null && pd.PompeID != null)
            .AsNoTracking();

        // Apply filters based on Periode.DateDebut
        if (date.HasValue)
        {
            var dateTime = date.Value.ToDateTime(TimeOnly.MinValue);
            var nextDay = dateTime.AddDays(1);
            query = query.Where(pd => pd.Periode!.DateDebut >= dateTime && pd.Periode.DateDebut < nextDay);
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(pd => pd.Periode!.DateDebut >= startDateTime && pd.Periode.DateDebut <= endDateTime);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            var startDateTime = monthStart.ToDateTime(TimeOnly.MinValue);
            var endDateTime = monthEnd.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(pd => pd.Periode!.DateDebut >= startDateTime && pd.Periode.DateDebut <= endDateTime);
        }
        else if (year.HasValue)
        {
            var yearStart = new DateTime(year.Value, 1, 1);
            var yearEnd = new DateTime(year.Value, 12, 31, 23, 59, 59);
            query = query.Where(pd => pd.Periode!.DateDebut >= yearStart && pd.Periode.DateDebut <= yearEnd);
        }

        var results = await query
            .OrderByDescending(pd => pd.Periode!.DateDebut)
            .Select(pd => new VenteCarburantAnalyticsRowDto
            {
                // Periode info
                PeriodeId = pd.PeriodeID,
                DateDebut = pd.Periode!.DateDebut,
                DateFin = pd.Periode.DateFin,
                
                // Product/Fuel info
                ProduitId = pd.ProduitID!.Value,
                ProduitNom = pd.Produit!.Description ?? pd.Produit.NumeroProduit,
                
                // Pump info
                PompeId = pd.PompeID!.Value,
                PompeNumero = pd.Pompe!.Numero ?? $"Pompe {pd.PompeID}",
                
                // Reservoir info
                ReservoirId = pd.ReservoirID ?? 0,
                ReservoirNumero = pd.Reservoir != null ? (pd.Reservoir.Numero ?? $"Reservoir {pd.ReservoirID}") : "",
                
                // Price
                PrixCarburant = pd.PrixCarburant,
                
                // Electronic meter readings
                CompteurElectroniqueDebut = pd.CompteurElectroniqueDebut,
                CompteurElectroniqueFinal = pd.CompteurElectroniqueFinal,
                QuantiteElectronique = pd.CompteurElectroniqueFinal - pd.CompteurElectroniqueDebut,
                PrixTotalElectronique = (pd.CompteurElectroniqueFinal - pd.CompteurElectroniqueDebut) * pd.PrixCarburant,
                
                // Mechanical meter readings
                CompteurMecaniqueDebut = pd.CompteurMecaniqueDebut,
                CompteurMecaniqueFinal = pd.CompteurMecaniqueFinal,
                QuantiteMecanique = pd.CompteurMecaniqueFinal - pd.CompteurMecaniqueDebut,
                PrixTotalMecanique = (pd.CompteurMecaniqueFinal - pd.CompteurMecaniqueDebut) * pd.PrixCarburant,
                
                // Difference
                DifferenceQuantite = (pd.CompteurElectroniqueFinal - pd.CompteurElectroniqueDebut) - 
                                    (pd.CompteurMecaniqueFinal - pd.CompteurMecaniqueDebut),
                DifferenceValeur = ((pd.CompteurElectroniqueFinal - pd.CompteurElectroniqueDebut) - 
                                   (pd.CompteurMecaniqueFinal - pd.CompteurMecaniqueDebut)) * pd.PrixCarburant,
                
                // Employee
                EmployeNom = pd.Periode.Employe != null ? pd.Periode.Employe.Nom : null
            })
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>
    /// Parses month string (yyyy-MM) into start and end DateOnly.
    /// </summary>
    private static bool TryParseMonth(string month, out DateOnly start, out DateOnly end)
    {
        start = default;
        end = default;

        var parts = month.Split('-');
        if (parts.Length != 2) return false;

        if (!int.TryParse(parts[0], out var y) || !int.TryParse(parts[1], out var m))
            return false;

        if (m < 1 || m > 12) return false;

        start = new DateOnly(y, m, 1);
        end = new DateOnly(y, m, DateTime.DaysInMonth(y, m));
        return true;
    }
}
