using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

/// <summary>
/// Controller for managing cash deposits (DepotCaisse) and cash summary.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DepotCaissesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    // ID for "Espèces" payment method (seeded as ID=1)
    private const int ESPECES_MOYEN_PAIEMENT_ID = 1;

    public DepotCaissesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all cash deposits.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepotCaisseDto>>> GetAll()
    {
        var depots = await _context.DepotsCaisse
            .Include(d => d.Validateur)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();

        return Ok(_mapper.Map<List<DepotCaisseDto>>(depots));
    }

    /// <summary>
    /// Gets a specific cash deposit by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DepotCaisseDto>> GetById(int id)
    {
        var depot = await _context.DepotsCaisse
            .Include(d => d.Validateur)
            .FirstOrDefaultAsync(d => d.ID == id);

        if (depot == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<DepotCaisseDto>(depot));
    }

    /// <summary>
    /// Gets cash deposits within a date range.
    /// </summary>
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<DepotCaisseDto>>> GetByDateRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var depots = await _context.DepotsCaisse
            .Include(d => d.Validateur)
            .Where(d => d.DateDepot >= start && d.DateDepot <= end)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();

        return Ok(_mapper.Map<List<DepotCaisseDto>>(depots));
    }

    /// <summary>
    /// Gets a summary of all cash in the station.
    /// Calculates from Periodes.Especes, VenteLubrifiantsEtArticles, VenteServices, and ReglementCredits
    /// where MoyenPaiementID = 1 (Espèces).
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<CaisseSummaryDto>> GetCashSummary(
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        var summary = new CaisseSummaryDto
        {
            DateDebut = start,
            DateFin = end
        };

        // Build base queries
        IQueryable<Periode> periodesQuery = _context.Periodes;
        IQueryable<VenteLubrifiantsEtArticles> venteLubQuery = _context.VenteLubrifiantsEtArticles;
        IQueryable<VenteService> venteServiceQuery = _context.VenteServices;
        IQueryable<ReglementCredit> reglementQuery = _context.ReglementsCredit;
        IQueryable<DepotCaisse> depotsQuery = _context.DepotsCaisse;

        // Apply date filters if provided
        if (start.HasValue)
        {
            periodesQuery = periodesQuery.Where(p => p.DateDebut >= start.Value);
            venteLubQuery = venteLubQuery.Where(v => v.DateVente >= start.Value);
            venteServiceQuery = venteServiceQuery.Where(v => v.DateVente >= start.Value);
            reglementQuery = reglementQuery.Where(r => r.DateReglement >= start.Value);
            depotsQuery = depotsQuery.Where(d => d.DateDepot >= start.Value);
        }

        if (end.HasValue)
        {
            periodesQuery = periodesQuery.Where(p => p.DateDebut <= end.Value);
            venteLubQuery = venteLubQuery.Where(v => v.DateVente <= end.Value);
            venteServiceQuery = venteServiceQuery.Where(v => v.DateVente <= end.Value);
            reglementQuery = reglementQuery.Where(r => r.DateReglement <= end.Value);
            depotsQuery = depotsQuery.Where(d => d.DateDepot <= end.Value);
        }

        // Calculate totals
        // 1. Espèces from Periodes
        summary.EspecesPeriodes = await periodesQuery.SumAsync(p => p.Especes);

        // 2. Espèces from VenteLubrifiantsEtArticles (where MoyenPaiementID = 1)
        summary.EspecesVenteLubArticles = await venteLubQuery
            .Where(v => v.MoyenPaiementID == ESPECES_MOYEN_PAIEMENT_ID)
            .SumAsync(v => v.PrixUnitaireTTC * v.QuantiteVendue);

        // 3. Espèces from VenteServices (where MoyenPaiementID = 1)
        summary.EspecesVenteServices = await venteServiceQuery
            .Where(v => v.MoyenPaiementID == ESPECES_MOYEN_PAIEMENT_ID)
            .SumAsync(v => v.PrixUnitaire * v.Quantite);

        // 4. Espèces from ReglementCredits (where ModePaiementID = 1)
        summary.EspecesReglementCredits = await reglementQuery
            .Where(r => r.ModePaiementID == ESPECES_MOYEN_PAIEMENT_ID)
            .SumAsync(r => r.MontantPaye);

        // 5. Total deposits to bank
        summary.TotalDepots = await depotsQuery.SumAsync(d => d.Montant);

        return Ok(summary);
    }

    /// <summary>
    /// Creates a new cash deposit.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DepotCaisseDto>> Create(DepotCaisseDto dto)
    {
        var depot = _mapper.Map<DepotCaisse>(dto);
        depot.DateCreation = DateTime.UtcNow;

        _context.DepotsCaisse.Add(depot);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(depot).Reference(d => d.Validateur).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = depot.ID }, _mapper.Map<DepotCaisseDto>(depot));
    }

    /// <summary>
    /// Updates an existing cash deposit.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, DepotCaisseDto dto)
    {
        if (id != dto.ID)
        {
            return BadRequest("ID mismatch");
        }

        var depot = await _context.DepotsCaisse.FindAsync(id);
        if (depot == null)
        {
            return NotFound();
        }

        // Update properties
        depot.Montant = dto.Montant;
        depot.DateDepot = dto.DateDepot;
        depot.ReferenceBancaire = dto.ReferenceBancaire;
        depot.Banque = dto.Banque;
        depot.Notes = dto.Notes;
        depot.PieceJustificativeNom = dto.PieceJustificativeNom;
        depot.PieceJustificativeBase64 = dto.PieceJustificativeBase64;
        depot.PieceJustificativeType = dto.PieceJustificativeType;
        depot.ValidePar = dto.ValidePar;
        depot.ModifiePar = dto.ModifiePar;
        depot.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a cash deposit.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var depot = await _context.DepotsCaisse.FindAsync(id);
        if (depot == null)
        {
            return NotFound();
        }

        _context.DepotsCaisse.Remove(depot);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
