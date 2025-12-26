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
    /// Get Jaugeage analysis comparing 2 jaugeages around the selected date/period vs actual sales.
    /// Detects "Remise au cuve" or stock discrepancies.
    /// Filters: date (specific day), month (yyyy-MM format)
    /// - For specific date: finds jaugeage (n) on or before date, and jaugeage (n-1) before that
    /// - For month: finds jaugeage (n) at end of month or closest before, and jaugeage (n-1) before that
    /// - No filter: uses last 2 jaugeages
    /// </summary>
    [HttpGet("jaugeage-analyse")]
    public async Task<ActionResult<object>> GetJaugeageAnalyse(
        [FromQuery] DateOnly? date = null,
        [FromQuery] string? month = null)
    {
        DateTime? filterEndDate = null;
        
        // Determine the reference date for finding jaugeages
        if (date.HasValue)
        {
            // For specific date: find jaugeage on or before this date
            filterEndDate = DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        }
        else if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var monthStart, out var monthEnd))
        {
            // For month: find jaugeage at end of month or closest before
            filterEndDate = DateTime.SpecifyKind(monthEnd.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        }

        // Find jaugeage (n) - the most recent one on or before the filter date
        var jaugeageActuelQuery = _context.Jaugeages
            .Include(j => j.Temoin)
            .AsNoTracking();

        if (filterEndDate.HasValue)
        {
            jaugeageActuelQuery = jaugeageActuelQuery
                .Where(j => j.DateJaugeage <= filterEndDate.Value);
        }

        var jaugeageActuel = await jaugeageActuelQuery
            .OrderByDescending(j => j.DateJaugeage)
            .FirstOrDefaultAsync();

        if (jaugeageActuel == null)
        {
            return Ok(new
            {
                HasData = false,
                Message = filterEndDate.HasValue 
                    ? "Aucun jaugeage trouve pour cette periode" 
                    : "Aucun jaugeage enregistre",
                JaugeageActuel = (object?)null,
                JaugeagePrecedent = (object?)null,
                Comparaisons = Array.Empty<object>()
            });
        }

        // Find jaugeage (n-1) - the one before jaugeage (n)
        var jaugeagePrecedent = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Where(j => j.DateJaugeage < jaugeageActuel.DateJaugeage)
            .OrderByDescending(j => j.DateJaugeage)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (jaugeagePrecedent == null)
        {
            return Ok(new
            {
                HasData = false,
                Message = "Il faut au moins 2 jaugeages pour effectuer l'analyse (jaugeage precedent manquant)",
                JaugeageActuel = new
                {
                    jaugeageActuel.ID,
                    jaugeageActuel.NumeroJaugeage,
                    jaugeageActuel.DateJaugeage,
                    TemoinNom = jaugeageActuel.Temoin?.Nom ?? "Inconnu"
                },
                JaugeagePrecedent = (object?)null,
                Comparaisons = Array.Empty<object>()
            });
        }

        // Get details for both jaugeages
        var jaugeageActuelDetails = await _context.JaugeageDetails
            .Include(jd => jd.Reservoir)
                .ThenInclude(r => r!.Produit)
            .Where(jd => jd.JaugeageID == jaugeageActuel.ID)
            .AsNoTracking()
            .ToListAsync();

        var jaugeagePrecedentDetails = await _context.JaugeageDetails
            .Include(jd => jd.Reservoir)
                .ThenInclude(r => r!.Produit)
            .Where(jd => jd.JaugeageID == jaugeagePrecedent.ID)
            .AsNoTracking()
            .ToListAsync();

        // Get sales (PeriodeDetails) between the two jaugeage dates, grouped by ReservoirID
        var startDateUtc = DateTime.SpecifyKind(jaugeagePrecedent.DateJaugeage, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(jaugeageActuel.DateJaugeage, DateTimeKind.Utc);

        var salesByReservoir = await _context.PeriodeDetails
            .Include(pd => pd.Periode)
            .Where(pd => pd.ReservoirID != null &&
                         pd.Periode!.DateDebut >= startDateUtc &&
                         pd.Periode.DateDebut <= endDateUtc)
            .GroupBy(pd => pd.ReservoirID)
            .Select(g => new
            {
                ReservoirID = g.Key,
                TotalVendu = g.Sum(pd => pd.CompteurElectroniqueFinal - pd.CompteurElectroniqueDebut)
            })
            .AsNoTracking()
            .ToListAsync();

        // Build comparison per reservoir
        var comparaisons = new List<object>();

        // Get all unique reservoir IDs from both jaugeages
        var allReservoirIds = jaugeageActuelDetails.Select(d => d.ReservoirID)
            .Union(jaugeagePrecedentDetails.Select(d => d.ReservoirID))
            .Distinct();

        foreach (var reservoirId in allReservoirIds)
        {
            var detailActuel = jaugeageActuelDetails.FirstOrDefault(d => d.ReservoirID == reservoirId);
            var detailPrecedent = jaugeagePrecedentDetails.FirstOrDefault(d => d.ReservoirID == reservoirId);

            if (detailActuel == null || detailPrecedent == null)
                continue;

            var volumePrecedent = detailPrecedent.VolumeCalcule;
            var volumeActuel = detailActuel.VolumeCalcule;
            var stockConsomme = volumePrecedent - volumeActuel; // What was consumed according to jaugeage

            var venteData = salesByReservoir.FirstOrDefault(s => s.ReservoirID == reservoirId);
            var quantiteVendue = venteData?.TotalVendu ?? 0;

            var ecart = stockConsomme - quantiteVendue;
            var ecartPourcentage = quantiteVendue != 0 ? (ecart / quantiteVendue) * 100 : 0;

            // Determine status
            string statut;
            if (Math.Abs(ecart) < 5) // Tolerance of 5L
                statut = "Normal";
            else if (ecart < 0)
                statut = "Remise au cuve"; // More in tank than expected (negative ecart means volume increased or less consumed than sold)
            else
                statut = "Manquant"; // Less in tank than expected

            comparaisons.Add(new
            {
                ReservoirId = reservoirId,
                ReservoirNumero = detailActuel.Reservoir?.Numero ?? $"Reservoir {reservoirId}",
                ProduitNom = detailActuel.Reservoir?.Produit?.Description ?? "Inconnu",
                VolumePrecedent = volumePrecedent,
                VolumeActuel = volumeActuel,
                StockConsomme = stockConsomme,
                QuantiteVendue = quantiteVendue,
                Ecart = ecart,
                EcartPourcentage = Math.Round(ecartPourcentage, 2),
                Statut = statut
            });
        }

        return Ok(new
        {
            HasData = true,
            JaugeageActuel = new
            {
                jaugeageActuel.ID,
                jaugeageActuel.NumeroJaugeage,
                jaugeageActuel.DateJaugeage,
                TemoinNom = jaugeageActuel.Temoin?.Nom ?? "Inconnu"
            },
            JaugeagePrecedent = new
            {
                jaugeagePrecedent.ID,
                jaugeagePrecedent.NumeroJaugeage,
                jaugeagePrecedent.DateJaugeage,
                TemoinNom = jaugeagePrecedent.Temoin?.Nom ?? "Inconnu"
            },
            PeriodeAnalyse = $"{jaugeagePrecedent.DateJaugeage:dd/MM/yyyy HH:mm} - {jaugeageActuel.DateJaugeage:dd/MM/yyyy HH:mm}",
            Comparaisons = comparaisons.OrderBy(c => ((dynamic)c).ReservoirNumero).ToList()
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
