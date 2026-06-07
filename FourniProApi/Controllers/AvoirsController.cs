using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvoirsController(
    AppDbContext context,
    IMapper mapper,
    ILogger<AvoirsController> logger)
    : ControllerBase
{
    // GET: api/Avoirs?venteId=5
    // GET: api/Avoirs?creditId=3
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvoirDto>>> GetAvoirs(
        [FromQuery] int? venteId,
        [FromQuery] int? creditId)
    {
        var query = context.Avoirs.AsNoTracking();

        if (venteId.HasValue)
            query = query.Where(a => a.VenteId == venteId.Value);
        if (creditId.HasValue)
            query = query.Where(a => a.CreditId == creditId.Value);

        var avoirs = await query
            .OrderByDescending(a => a.DateAvoir)
            .ToListAsync();

        var dtos = mapper.Map<List<AvoirDto>>(avoirs);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Avoirs/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AvoirDto>> GetAvoir(int id)
    {
        var avoir = await context.Avoirs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AvoirId == id);

        if (avoir is null) return NotFound();

        var dto = mapper.Map<AvoirDto>(avoir);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Avoirs
    [HttpPost]
    public async Task<ActionResult<AvoirDto>> CreateAvoir(AvoirDto dto)
    {
        var validationError = await ValidateSourceAsync(dto);
        if (validationError is not null) return BadRequest(validationError);

        var avoir = mapper.Map<Avoir>(dto);
        avoir.DateCreation     = DateTime.UtcNow;
        avoir.DateModification = DateTime.UtcNow;
        avoir.MontantAvoir     = await ComputeMontantAsync(dto);

        // Enforce Vente-only fields when source is Credit
        if (dto.CreditId.HasValue)
        {
            avoir.Quantite     = null;
            avoir.PrixUnitaire = null;
            avoir.ProduitId    = null;
        }

        context.Avoirs.Add(avoir);
        await context.SaveChangesAsync();

        avoir.NumeroAvoir = $"AVR-{avoir.DateAvoir:yyyyMMdd}-{avoir.AvoirId:D5}";
        await context.SaveChangesAsync();

        logger.LogInformation("Avoir créé: {Numero} (Id={Id})", avoir.NumeroAvoir, avoir.AvoirId);

        var result = mapper.Map<AvoirDto>(avoir);
        await ResolveNamesAsync([result]);
        return CreatedAtAction(nameof(GetAvoir), new { id = avoir.AvoirId }, result);
    }

    // PUT: api/Avoirs/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AvoirDto>> UpdateAvoir(int id, AvoirDto dto)
    {
        var avoir = await context.Avoirs.FindAsync(id);
        if (avoir is null) return NotFound();

        var validationError = await ValidateSourceAsync(dto, excludeId: id);
        if (validationError is not null) return BadRequest(validationError);

        mapper.Map(dto, avoir);
        avoir.AvoirId         = id;
        avoir.DateModification = DateTime.UtcNow;
        avoir.MontantAvoir    = await ComputeMontantAsync(dto);

        // Enforce Vente-only fields when source is Credit
        if (dto.CreditId.HasValue)
        {
            avoir.Quantite     = null;
            avoir.PrixUnitaire = null;
            avoir.ProduitId    = null;
        }

        await context.SaveChangesAsync();

        var result = mapper.Map<AvoirDto>(avoir);
        await ResolveNamesAsync([result]);
        return Ok(result);
    }

    // DELETE: api/Avoirs/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAvoir(int id)
    {
        var avoir = await context.Avoirs.FindAsync(id);
        if (avoir is null) return NotFound();

        context.Avoirs.Remove(avoir);
        await context.SaveChangesAsync();

        logger.LogInformation("Avoir supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Business logic ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns an error message string when the DTO is invalid, null when valid.
    /// Rules enforced:
    ///   - Exactly one of VenteId / CreditId must be set.
    ///   - TropPercu is only valid when source is Credit.
    ///   - Referenced Vente / Credit must exist.
    /// </summary>
    private async Task<string?> ValidateSourceAsync(AvoirDto dto, int? excludeId = null)
    {
        bool hasVente  = dto.VenteId.HasValue;
        bool hasCredit = dto.CreditId.HasValue;

        if (hasVente == hasCredit)
            return "Exactly one of VenteId or CreditId must be provided.";

        if (hasVente)
        {
            if (!await context.Ventes.AnyAsync(v => v.VenteId == dto.VenteId!.Value))
                return $"Vente {dto.VenteId} not found.";

            if (string.Equals(dto.Raison, nameof(AvoirRaison.TropPercu), StringComparison.OrdinalIgnoreCase))
                return "Raison 'TropPercu' is only valid for Credit-source avoirs.";
        }

        if (hasCredit)
        {
            if (!await context.Credits.AnyAsync(c => c.CreditId == dto.CreditId!.Value))
                return $"Credit {dto.CreditId} not found.";
        }

        return null;
    }

    /// <summary>
    /// Computes MontantAvoir server-side, ignoring whatever the client sent.
    /// From Vente  → Quantite × PrixUnitaire (from the DTO snapshot).
    /// From Credit → SUM(PaiementCredit.Montant) − Credit.MontantTotal.
    /// </summary>
    private async Task<decimal?> ComputeMontantAsync(AvoirDto dto)
    {
        if (dto.VenteId.HasValue)
        {
            var q = dto.Quantite ?? 0;
            var p = dto.PrixUnitaire ?? 0m;
            return q * p;
        }

        if (dto.CreditId.HasValue)
        {
            var credit = await context.Credits
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CreditId == dto.CreditId.Value);

            if (credit is null) return null;

            var totalPaye = await context.PaiementsCredit
                .AsNoTracking()
                .Where(p => p.CreditId == dto.CreditId.Value)
                .SumAsync(p => (decimal?)(p.Montant ?? 0m)) ?? 0m;

            var surplus = totalPaye - (credit.MontantTotal ?? 0m);
            return surplus > 0 ? surplus : 0m;
        }

        return null;
    }

    // ── Name resolution ───────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<AvoirDto> dtos)
    {
        // Ventes
        var venteIds = dtos.Where(d => d.VenteId.HasValue).Select(d => d.VenteId!.Value).Distinct().ToList();
        var ventes = await context.Ventes
            .AsNoTracking()
            .Where(v => venteIds.Contains(v.VenteId))
            .Select(v => new { v.VenteId, v.NumeroVente })
            .ToDictionaryAsync(v => v.VenteId);

        // Credits
        var creditIds = dtos.Where(d => d.CreditId.HasValue).Select(d => d.CreditId!.Value).Distinct().ToList();
        var credits = await context.Credits
            .AsNoTracking()
            .Where(c => creditIds.Contains(c.CreditId))
            .Select(c => new { c.CreditId, c.NumeroCredit })
            .ToDictionaryAsync(c => c.CreditId);

        // Produits
        var produitIds = dtos.Where(d => d.ProduitId.HasValue).Select(d => d.ProduitId!.Value).Distinct().ToList();
        var produits = await context.Produits
            .AsNoTracking()
            .Where(p => produitIds.Contains(p.ProduitId))
            .Select(p => new { p.ProduitId, Nom = p.NumeroProduit })
            .ToDictionaryAsync(p => p.ProduitId);

        // Clients
        var clientIds = dtos.Where(d => d.ClientId.HasValue).Select(d => d.ClientId!.Value).Distinct().ToList();
        var clients = await context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.ClientId))
            .Select(c => new { c.ClientId, c.Nom })
            .ToDictionaryAsync(c => c.ClientId);

        // Employes
        var employeIds = dtos.Where(d => d.EmployeId.HasValue).Select(d => d.EmployeId!.Value).Distinct().ToList();
        var employes = await context.Employes
            .AsNoTracking()
            .Where(e => employeIds.Contains(e.EmployeId))
            .Select(e => new { e.EmployeId, Nom = e.Prenom + " " + e.Nom })
            .ToDictionaryAsync(e => e.EmployeId);

        // Users (audit)
        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue).Select(id => id!.Value)
            .Distinct().ToList();
        var users = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.Name })
            .ToDictionaryAsync(u => u.UserId);

        foreach (var dto in dtos)
        {
            if (dto.VenteId.HasValue && ventes.TryGetValue(dto.VenteId.Value, out var v))
                dto.NumeroVente = v.NumeroVente;

            if (dto.CreditId.HasValue && credits.TryGetValue(dto.CreditId.Value, out var c))
                dto.NumeroCredit = c.NumeroCredit;

            if (dto.ProduitId.HasValue && produits.TryGetValue(dto.ProduitId.Value, out var p))
                dto.ProduitNom = p.Nom;

            if (dto.ClientId.HasValue && clients.TryGetValue(dto.ClientId.Value, out var cl))
                dto.ClientNom = cl.Nom;

            if (dto.EmployeId.HasValue && employes.TryGetValue(dto.EmployeId.Value, out var e))
                dto.EmployeNom = e.Nom;

            if (dto.AjoutePar.HasValue && users.TryGetValue(dto.AjoutePar.Value, out var ua))
                dto.AjouteParNom = ua.Name;

            if (dto.ModifiePar.HasValue && users.TryGetValue(dto.ModifiePar.Value, out var um))
                dto.ModifieParNom = um.Name;
        }
    }
}
