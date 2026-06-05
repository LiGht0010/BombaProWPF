using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentesController(AppDbContext context, IMapper mapper, ILogger<VentesController> logger)
    : ControllerBase
{
    // GET: api/Ventes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenteDto>>> GetVentes()
    {
        var ventes = await context.Ventes
            .AsNoTracking()
            .OrderByDescending(v => v.DateVente)
            .ToListAsync();

        var dtos = mapper.Map<List<VenteDto>>(ventes);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Ventes/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VenteDto>> GetVente(int id)
    {
        var vente = await context.Ventes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VenteId == id);

        if (vente is null) return NotFound();

        var dto = mapper.Map<VenteDto>(vente);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Ventes
    [HttpPost]
    public async Task<ActionResult<VenteDto>> CreateVente(VenteDto dto)
    {
        var vente = mapper.Map<Vente>(dto);
        vente.VenteId        = 0;
        vente.DateCreation   = DateTime.UtcNow;
        vente.DateModification = DateTime.UtcNow;
        vente.MontantTotal   = ComputeTotal(dto);

        context.Ventes.Add(vente);
        await context.SaveChangesAsync();

        // Generate NumeroVente after we have the PK
        vente.NumeroVente = $"VNT-{vente.DateVente:yyyyMMdd}-{vente.VenteId:D5}";
        await context.SaveChangesAsync();

        logger.LogInformation("Vente créée: {Numero} (Id={Id})", vente.NumeroVente, vente.VenteId);
        return CreatedAtAction(nameof(GetVente), new { id = vente.VenteId }, mapper.Map<VenteDto>(vente));
    }

    // PUT: api/Ventes/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVente(int id, VenteDto dto)
    {
        var vente = await context.Ventes.FindAsync(id);
        if (vente is null) return NotFound();

        mapper.Map(dto, vente);
        vente.VenteId          = id;
        vente.DateModification = DateTime.UtcNow;
        vente.MontantTotal     = ComputeTotal(dto);

        await context.SaveChangesAsync();

        logger.LogInformation("Vente mise à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Ventes/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVente(int id)
    {
        var vente = await context.Ventes.FindAsync(id);
        if (vente is null) return NotFound();

        context.Ventes.Remove(vente);
        await context.SaveChangesAsync();

        logger.LogInformation("Vente supprimée: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Computes MontantTotal = PrixUnitaire × Quantite − Remise.</summary>
    private static decimal? ComputeTotal(VenteDto dto)
    {
        if (dto.PrixUnitaire is null || dto.Quantite is null)
            return null;

        var total = dto.PrixUnitaire.Value * dto.Quantite.Value;
        if (dto.Remise.HasValue)
            total -= dto.Remise.Value;

        return Math.Max(0, total);
    }

    private async Task ResolveNamesAsync(IList<VenteDto> dtos)
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
