using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AchatsController(AppDbContext context, IMapper mapper, ILogger<AchatsController> logger)
    : ControllerBase
{
    // GET: api/Achats
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AchatDto>>> GetAchats()
    {
        var achats = await context.Achats.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<AchatDto>>(achats);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Achats/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AchatDto>> GetAchat(int id)
    {
        var achat = await context.Achats.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AchatId == id);

        if (achat is null) return NotFound();

        var dto = mapper.Map<AchatDto>(achat);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Achats
    [HttpPost]
    public async Task<ActionResult<AchatDto>> CreateAchat(AchatDto dto)
    {
        var achat = mapper.Map<Achat>(dto);
        achat.DateCreation = DateTime.UtcNow;
        achat.DateModification = DateTime.UtcNow;

        context.Achats.Add(achat);
        await context.SaveChangesAsync();

        logger.LogInformation("Achat créé: {Numero} (Id={Id})", achat.Numero, achat.AchatId);
        return CreatedAtAction(nameof(GetAchat), new { id = achat.AchatId }, mapper.Map<AchatDto>(achat));
    }

    // PUT: api/Achats/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAchat(int id, AchatDto dto)
    {
        var achat = await context.Achats.FindAsync(id);
        if (achat is null) return NotFound();

        mapper.Map(dto, achat);
        achat.AchatId = id;
        achat.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Achat mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Achats/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAchat(int id)
    {
        var achat = await context.Achats.FindAsync(id);
        if (achat is null) return NotFound();

        context.Achats.Remove(achat);
        await context.SaveChangesAsync();

        logger.LogInformation("Achat supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<AchatDto> dtos)
    {
        var fournisseurIds = dtos
            .Where(d => d.FournisseurID.HasValue)
            .Select(d => d.FournisseurID!.Value)
            .Distinct()
            .ToList();

        var produitIds = dtos
            .Where(d => d.ProduitID.HasValue)
            .Select(d => d.ProduitID!.Value)
            .Distinct()
            .ToList();

        var employeIds = dtos
            .Where(d => d.EmployeId.HasValue)
            .Select(d => d.EmployeId!.Value)
            .Distinct()
            .ToList();

        var voyageIds = dtos
            .Where(d => d.VoyageID.HasValue)
            .Select(d => d.VoyageID!.Value)
            .Distinct()
            .ToList();

        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var fournisseurNames = fournisseurIds.Count > 0
            ? await context.Fournisseurs
                .AsNoTracking()
                .Where(f => fournisseurIds.Contains(f.FournisseurId))
                .ToDictionaryAsync(f => f.FournisseurId, f => f.Nom)
            : [];

        var produitNames = produitIds.Count > 0
            ? await context.Produits
                .AsNoTracking()
                .Where(p => produitIds.Contains(p.ProduitId))
                .ToDictionaryAsync(p => p.ProduitId, p =>
                    !string.IsNullOrWhiteSpace(p.Description) ? p.Description : p.NumeroProduit)
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
            if (dto.FournisseurID.HasValue && fournisseurNames.TryGetValue(dto.FournisseurID.Value, out var fNom))
                dto.FournisseurNom = fNom;
            if (dto.ProduitID.HasValue && produitNames.TryGetValue(dto.ProduitID.Value, out var pNom))
                dto.ProduitNom = pNom;
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
