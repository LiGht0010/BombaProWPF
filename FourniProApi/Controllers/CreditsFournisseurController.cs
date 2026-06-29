using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditsFournisseurController(
    AppDbContext context,
    IMapper mapper,
    ILogger<CreditsFournisseurController> logger)
    : ControllerBase
{
    // GET: api/CreditsFournisseur?fournisseurId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditFournisseurDto>>> GetCredits(
        [FromQuery] int? fournisseurId)
    {
        var query = context.CreditsFournisseur.AsNoTracking();

        if (fournisseurId.HasValue)
            query = query.Where(c => c.FournisseurId == fournisseurId.Value);

        var credits = await query
            .OrderByDescending(c => c.DateCredit)
            .ToListAsync();

        var dtos = mapper.Map<List<CreditFournisseurDto>>(credits);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/CreditsFournisseur/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CreditFournisseurDto>> GetCredit(int id)
    {
        var credit = await context.CreditsFournisseur
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CreditFournisseurId == id);

        if (credit is null) return NotFound();

        var dto = mapper.Map<CreditFournisseurDto>(credit);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/CreditsFournisseur
    [HttpPost]
    public async Task<ActionResult<CreditFournisseurDto>> CreateCredit(CreditFournisseurDto dto)
    {
        var credit = mapper.Map<CreditFournisseur>(dto);
        credit.DateCreation     = DateTime.UtcNow;
        credit.DateModification = DateTime.UtcNow;
        credit.Statut           = nameof(CreditFournisseurStatut.NonPayé);
        credit.MontantTotal     = await ComputeTotalAsync(dto.AchatId);

        context.CreditsFournisseur.Add(credit);
        await context.SaveChangesAsync();

        // Generate NumeroCreditF after we have the PK
        credit.NumeroCreditF = $"CF-{credit.DateCredit:yyyyMMdd}-{credit.CreditFournisseurId:D5}";
        await context.SaveChangesAsync();

        logger.LogInformation("CréditFournisseur créé: {Numero} (Id={Id})",
            credit.NumeroCreditF, credit.CreditFournisseurId);

        var result = mapper.Map<CreditFournisseurDto>(credit);
        await ResolveNamesAsync([result]);
        return CreatedAtAction(nameof(GetCredit), new { id = credit.CreditFournisseurId }, result);
    }

    // PUT: api/CreditsFournisseur/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCredit(int id, CreditFournisseurDto dto)
    {
        var credit = await context.CreditsFournisseur.FindAsync(id);
        if (credit is null) return NotFound();

        mapper.Map(dto, credit);
        credit.CreditFournisseurId = id;
        credit.DateModification    = DateTime.UtcNow;
        credit.MontantTotal        = await ComputeTotalAsync(dto.AchatId);

        await context.SaveChangesAsync();

        logger.LogInformation("CréditFournisseur mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/CreditsFournisseur/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCredit(int id)
    {
        var credit = await context.CreditsFournisseur.FindAsync(id);
        if (credit is null) return NotFound();

        context.CreditsFournisseur.Remove(credit);
        await context.SaveChangesAsync();

        logger.LogInformation("CréditFournisseur supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches Achat.Cout as MontantTotal; returns null when AchatId is absent or Cout is null.
    /// </summary>
    private async Task<decimal?> ComputeTotalAsync(int? achatId)
    {
        if (achatId is null) return null;

        return await context.Achats
            .AsNoTracking()
            .Where(a => a.AchatId == achatId.Value)
            .Select(a => a.Cout)
            .FirstOrDefaultAsync();
    }

    private async Task ResolveNamesAsync(IList<CreditFournisseurDto> dtos)
    {
        var achatIds = dtos
            .Where(d => d.AchatId.HasValue)
            .Select(d => d.AchatId!.Value)
            .Distinct().ToList();

        var fournisseurIds = dtos
            .Where(d => d.FournisseurId.HasValue)
            .Select(d => d.FournisseurId!.Value)
            .Distinct().ToList();

        var employeIds = dtos
            .Where(d => d.EmployeId.HasValue)
            .Select(d => d.EmployeId!.Value)
            .Distinct().ToList();

        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct().ToList();

        var achatNumeros = achatIds.Count > 0
            ? await context.Achats
                .AsNoTracking()
                .Where(a => achatIds.Contains(a.AchatId))
                .ToDictionaryAsync(a => a.AchatId, a => a.Numero)
            : [];

        var fournisseurNoms = fournisseurIds.Count > 0
            ? await context.Fournisseurs
                .AsNoTracking()
                .Where(f => fournisseurIds.Contains(f.FournisseurId))
                .ToDictionaryAsync(f => f.FournisseurId,
                    f => !string.IsNullOrWhiteSpace(f.Societe) ? f.Societe : $"{f.Prenom} {f.Nom}".Trim())
            : [];

        var employeNames = employeIds.Count > 0
            ? await context.Employes
                .AsNoTracking()
                .Where(e => employeIds.Contains(e.EmployeId))
                .ToDictionaryAsync(e => e.EmployeId, e => $"{e.Prenom} {e.Nom}")
            : [];

        var userNames = userIds.Count > 0
            ? await context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.Name)
            : [];

        foreach (var dto in dtos)
        {
            if (dto.AchatId.HasValue && achatNumeros.TryGetValue(dto.AchatId.Value, out var aNum))
                dto.AchatNumero = aNum;
            if (dto.FournisseurId.HasValue && fournisseurNoms.TryGetValue(dto.FournisseurId.Value, out var fNom))
                dto.FournisseurNom = fNom;
            if (dto.EmployeId.HasValue && employeNames.TryGetValue(dto.EmployeId.Value, out var eNom))
                dto.EmployeNom = eNom;
            if (dto.AjoutePar.HasValue && userNames.TryGetValue(dto.AjoutePar.Value, out var ajouteNom))
                dto.AjouteParNom = ajouteNom;
            if (dto.ModifiePar.HasValue && userNames.TryGetValue(dto.ModifiePar.Value, out var modNom))
                dto.ModifieParNom = modNom;
        }
    }
}
