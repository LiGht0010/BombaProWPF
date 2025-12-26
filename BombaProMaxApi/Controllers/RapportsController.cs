using BombaProMaxApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RapportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RapportsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get sales report (Ventes Carburant + Ventes Lubrifiants/Articles).
    /// Filters: date (specific), month (yyyy-MM format)
    /// For Periode: filters by DateDebut
    /// </summary>
    [HttpGet("ventes")]
    public async Task<ActionResult<object>> GetRapportVentes(
        [FromQuery] DateOnly? date = null,
        [FromQuery] string? month = null)
    {
        // === VENTES CARBURANT (from PeriodeDetails, filter by Periode.DateDebut) ===
        var carburantQuery = _context.PeriodeDetails
            .Include(pd => pd.Periode)
            .Include(pd => pd.Produit)
            .Where(pd => pd.ProduitID != null)
            .AsNoTracking();

        if (date.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MinValue).AddDays(1), DateTimeKind.Utc);
            carburantQuery = carburantQuery.Where(pd => pd.Periode!.DateDebut >= startUtc && pd.Periode.DateDebut < endUtc);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            var startUtc = DateTime.SpecifyKind(monthStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(monthEnd.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
            carburantQuery = carburantQuery.Where(pd => pd.Periode!.DateDebut >= startUtc && pd.Periode.DateDebut <= endUtc);
        }

        var carburantData = await carburantQuery.ToListAsync();
        var carburantGrouped = carburantData
            .GroupBy(pd => pd.ProduitID)
            .Select(g => new
            {
                ProduitId = g.Key ?? 0,
                ProduitNom = g.First().Produit?.Description ?? g.First().Produit?.NumeroProduit ?? "Inconnu",
                TotalQuantite = g.Sum(x => x.CompteurElectroniqueFinal - x.CompteurElectroniqueDebut),
                TotalMontant = g.Sum(x => (x.CompteurElectroniqueFinal - x.CompteurElectroniqueDebut) * x.PrixCarburant),
                NombrePeriodes = g.Select(x => x.PeriodeID).Distinct().Count()
            })
            .OrderByDescending(x => x.TotalQuantite)
            .ToList();

        // === VENTES LUBRIFIANTS & ARTICLES (filter by DateVente) ===
        var lubQuery = _context.VenteLubrifiantsEtArticles
            .Include(v => v.Produit)
                .ThenInclude(p => p!.Categorie)
            .AsNoTracking();

        if (date.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MinValue).AddDays(1), DateTimeKind.Utc);
            lubQuery = lubQuery.Where(v => v.DateVente >= startUtc && v.DateVente < endUtc);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var mStart, out var mEnd))
        {
            var startUtc = DateTime.SpecifyKind(mStart.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(mEnd.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
            lubQuery = lubQuery.Where(v => v.DateVente >= startUtc && v.DateVente <= endUtc);
        }

        var lubData = await lubQuery.ToListAsync();
        var lubGrouped = lubData
            .GroupBy(v => v.ProduitID)
            .Select(g => new
            {
                ProduitId = g.Key,
                ProduitNom = g.First().Produit?.Description ?? g.First().Produit?.NumeroProduit ?? "Inconnu",
                CategorieNom = g.First().Produit?.Categorie?.Nom,
                TotalQuantite = g.Sum(x => x.QuantiteVendue),
                TotalMontant = g.Sum(x => x.PrixUnitaireTTC * x.QuantiteVendue),
                NombreVentes = g.Count()
            })
            .OrderByDescending(x => x.TotalQuantite)
            .ToList();

        return Ok(new
        {
            TotalVentesCarburant = carburantGrouped.Sum(x => x.TotalMontant),
            TotalQuantiteCarburant = carburantGrouped.Sum(x => x.TotalQuantite),
            VentesCarburantParProduit = carburantGrouped,
            TotalVentesLubArticles = lubGrouped.Sum(x => x.TotalMontant),
            TotalQuantiteLubArticles = lubGrouped.Sum(x => x.TotalQuantite),
            VentesLubArticlesParProduit = lubGrouped
        });
    }

    /// <summary>
    /// Get expenses report (Depenses grouped by category).
    /// Filters: date (specific), month (yyyy-MM format)
    /// Filters by Depense.Date
    /// </summary>
    [HttpGet("depenses")]
    public async Task<ActionResult<object>> GetRapportDepenses(
        [FromQuery] DateOnly? date = null,
        [FromQuery] string? month = null)
    {
        var query = _context.Depenses.AsNoTracking();

        if (date.HasValue)
        {
            query = query.Where(d => d.Date == date.Value);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            query = query.Where(d => d.Date >= monthStart && d.Date <= monthEnd);
        }

        var depenses = await query.OrderByDescending(d => d.Date).ToListAsync();

        var parCategorie = depenses
            .GroupBy(d => d.Categorie ?? "Non categorise")
            .Select(g => new
            {
                CategorieNom = g.Key,
                TotalMontant = g.Sum(x => x.Montant ?? 0),
                NombreDepenses = g.Count()
            })
            .OrderByDescending(x => x.TotalMontant)
            .ToList();

        var details = depenses.Select(d => new
        {
            d.ID,
            d.Numero,
            d.Date,
            d.Categorie,
            Montant = d.Montant ?? 0,
            d.Description
        }).ToList();

        return Ok(new
        {
            TotalDepenses = depenses.Sum(d => d.Montant ?? 0),
            NombreDepenses = depenses.Count,
            DepensesParCategorie = parCategorie,
            DepensesDetails = details
        });
    }

    /// <summary>
    /// Get stock report (Reservoirs + Products + Achats movements).
    /// Achats filter: date (specific), month (yyyy-MM format)
    /// Stock: Current state (no date filter)
    /// </summary>
    [HttpGet("stock")]
    public async Task<ActionResult<object>> GetRapportStock(
        [FromQuery] DateOnly? date = null,
        [FromQuery] string? month = null)
    {
        // === STOCK CARBURANT (Reservoirs - current state) ===
        var reservoirs = await _context.Reservoirs
            .Include(r => r.Produit)
            .AsNoTracking()
            .Select(r => new
            {
                ReservoirId = r.ID,
                ReservoirNumero = r.Numero ?? $"Reservoir {r.ID}",
                ProduitNom = r.Produit != null ? r.Produit.Description : null,
                Capacite = r.Capacite,
                NiveauActuel = r.NiveauDeCarburant
            })
            .ToListAsync();

        // === STOCK PRODUITS (Non-fuel products - current state) ===
        // Filter out products that are fuel (associated with reservoirs)
        var fuelProductIds = await _context.Reservoirs
            .Where(r => r.ProduitID != null)
            .Select(r => r.ProduitID)
            .Distinct()
            .ToListAsync();

        var produits = await _context.Produits
            .Include(p => p.Categorie)
            .Where(p => !fuelProductIds.Contains(p.ID))
            .AsNoTracking()
            .Select(p => new
            {
                ProduitId = p.ID,
                ProduitNom = p.Description ?? p.NumeroProduit,
                CategorieNom = p.Categorie != null ? p.Categorie.Nom : null,
                StockActuel = p.Stock ?? 0,
                StockMinimum = p.StockMinimum
            })
            .ToListAsync();

        // === ACHATS (Movements during period) ===
        var achatsQuery = _context.Achats
            .Include(a => a.Produit)
                .ThenInclude(p => p!.Categorie)
            .Where(a => a.ProduitID != null && a.Quantite != null)
            .AsNoTracking();

        if (date.HasValue)
        {
            achatsQuery = achatsQuery.Where(a => a.Date == date.Value);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            achatsQuery = achatsQuery.Where(a => a.Date >= monthStart && a.Date <= monthEnd);
        }

        var achatsData = await achatsQuery.ToListAsync();
        var achatsGrouped = achatsData
            .GroupBy(a => a.ProduitID)
            .Select(g => new
            {
                ProduitId = g.Key ?? 0,
                ProduitNom = g.First().Produit?.Description ?? g.First().Produit?.NumeroProduit ?? "Inconnu",
                CategorieNom = g.First().Produit?.Categorie?.Nom,
                TotalQuantite = g.Sum(x => x.Quantite ?? 0),
                TotalMontant = g.Sum(x => x.Cout ?? (x.Quantite ?? 0) * (x.PrixAchatUnitaire ?? 0)),
                NombreAchats = g.Count()
            })
            .OrderByDescending(x => x.TotalMontant)
            .ToList();

        return Ok(new
        {
            StockCarburant = reservoirs,
            TotalStockCarburantLitres = reservoirs.Sum(r => r.NiveauActuel),
            StockProduits = produits,
            TotalStockProduits = produits.Sum(p => p.StockActuel),
            TotalAchatsPeriode = achatsGrouped.Sum(a => a.TotalMontant),
            AchatsParProduit = achatsGrouped
        });
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
