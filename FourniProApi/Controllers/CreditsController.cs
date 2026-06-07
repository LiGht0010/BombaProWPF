using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditsController(AppDbContext context, IMapper mapper, ILogger<CreditsController> logger)
    : ControllerBase
{
    // GET: api/Credits
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditDto>>> GetCredits()
    {
        var credits = await context.Credits
            .AsNoTracking()
            .OrderByDescending(c => c.DateCredit)
            .ToListAsync();

        var dtos = mapper.Map<List<CreditDto>>(credits);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Credits/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CreditDto>> GetCredit(int id)
    {
        var credit = await context.Credits
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CreditId == id);

        if (credit is null) return NotFound();

        var dto = mapper.Map<CreditDto>(credit);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Credits
    [HttpPost]
    public async Task<ActionResult<CreditDto>> CreateCredit(CreditDto dto)
    {
        var credit = mapper.Map<Credit>(dto);
        credit.DateCreation     = DateTime.UtcNow;
        credit.DateModification = DateTime.UtcNow;
        credit.MontantTotal     = ComputeTotal(dto);

        context.Credits.Add(credit);
        await context.SaveChangesAsync();

        // Generate NumeroCredit after we have the PK
        credit.NumeroCredit = $"CRD-{credit.DateCredit:yyyyMMdd}-{credit.CreditId:D5}";
        await context.SaveChangesAsync();

        logger.LogInformation("Crédit créé: {Numero} (Id={Id})", credit.NumeroCredit, credit.CreditId);
        return CreatedAtAction(nameof(GetCredit), new { id = credit.CreditId }, mapper.Map<CreditDto>(credit));
    }

    // PUT: api/Credits/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCredit(int id, CreditDto dto)
    {
        var credit = await context.Credits.FindAsync(id);
        if (credit is null) return NotFound();

        mapper.Map(dto, credit);
        credit.CreditId         = id;
        credit.DateModification = DateTime.UtcNow;
        credit.MontantTotal     = ComputeTotal(dto);

        await context.SaveChangesAsync();

        logger.LogInformation("Crédit mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Credits/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCredit(int id)
    {
        var credit = await context.Credits.FindAsync(id);
        if (credit is null) return NotFound();

        context.Credits.Remove(credit);
        await context.SaveChangesAsync();

        logger.LogInformation("Crédit supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Computes MontantTotal = PrixUnitaire × Quantite − Remise.</summary>
    private static decimal? ComputeTotal(CreditDto dto)
    {
        if (dto.PrixUnitaire is null || dto.Quantite is null)
            return null;

        var total = dto.PrixUnitaire.Value * dto.Quantite.Value;
        if (dto.Remise.HasValue)
            total -= dto.Remise.Value;

        return Math.Max(0, total);
    }

    private async Task ResolveNamesAsync(IList<CreditDto> dtos)
    {
        var produitIds = dtos
            .Where(d => d.ProduitID.HasValue)
            .Select(d => d.ProduitID!.Value)
            .Distinct().ToList();

        var clientIds = dtos
            .Where(d => d.ClientID.HasValue)
            .Select(d => d.ClientID!.Value)
            .Distinct().ToList();

        var employeIds = dtos
            .Where(d => d.EmployeId.HasValue)
            .Select(d => d.EmployeId!.Value)
            .Distinct().ToList();

        var voyageIds = dtos
            .Where(d => d.VoyageID.HasValue)
            .Select(d => d.VoyageID!.Value)
            .Distinct().ToList();

        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct().ToList();

        var produitNames = produitIds.Count > 0
            ? await context.Produits
                .AsNoTracking()
                .Where(p => produitIds.Contains(p.ProduitId))
                .ToDictionaryAsync(p => p.ProduitId,
                    p => !string.IsNullOrWhiteSpace(p.Description) ? p.Description : p.NumeroProduit)
            : [];

        var clientNames = clientIds.Count > 0
            ? await context.Clients
                .AsNoTracking()
                .Where(c => clientIds.Contains(c.ClientId))
                .ToDictionaryAsync(c => c.ClientId, c => c.Nom)
            : [];

        var employeNames = employeIds.Count > 0
            ? await context.Employes
                .AsNoTracking()
                .Where(e => employeIds.Contains(e.EmployeId))
                .ToDictionaryAsync(e => e.EmployeId, e => $"{e.Prenom} {e.Nom}")
            : [];

        var voyageLabels = voyageIds.Count > 0
            ? await context.Voyages
                .AsNoTracking()
                .Where(v => voyageIds.Contains(v.VoyageId))
                .ToDictionaryAsync(v => v.VoyageId, v => $"VOY-{v.VoyageId:D5}")
            : [];

        var userNames = userIds.Count > 0
            ? await context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.Name)
            : [];

        foreach (var dto in dtos)
        {
            if (dto.ProduitID.HasValue && produitNames.TryGetValue(dto.ProduitID.Value, out var pNom))
                dto.ProduitNom = pNom;
            if (dto.ClientID.HasValue && clientNames.TryGetValue(dto.ClientID.Value, out var cNom))
                dto.ClientNom = cNom;
            if (dto.EmployeId.HasValue && employeNames.TryGetValue(dto.EmployeId.Value, out var eNom))
                dto.EmployeNom = eNom;
            if (dto.VoyageID.HasValue && voyageLabels.TryGetValue(dto.VoyageID.Value, out var vLabel))
                dto.VoyageNumero = vLabel;
            if (dto.AjoutePar.HasValue && userNames.TryGetValue(dto.AjoutePar.Value, out var ajouteNom))
                dto.AjouteParNom = ajouteNom;
            if (dto.ModifiePar.HasValue && userNames.TryGetValue(dto.ModifiePar.Value, out var modNom))
                dto.ModifieParNom = modNom;
        }
    }
}
