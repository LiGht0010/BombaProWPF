using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FournisseursController(AppDbContext context, IMapper mapper, ILogger<FournisseursController> logger)
    : ControllerBase
{
    // GET: api/Fournisseurs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FournisseurDto>>> GetFournisseurs()
    {
        var fournisseurs = await context.Fournisseurs.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<FournisseurDto>>(fournisseurs);
        await ResolveUserNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Fournisseurs/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FournisseurDto>> GetFournisseur(int id)
    {
        var fournisseur = await context.Fournisseurs.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FournisseurId == id);

        if (fournisseur is null) return NotFound();

        var dto = mapper.Map<FournisseurDto>(fournisseur);
        await ResolveUserNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Fournisseurs
    [HttpPost]
    public async Task<ActionResult<FournisseurDto>> CreateFournisseur(FournisseurDto dto)
    {
        var fournisseur = mapper.Map<Fournisseur>(dto);
        fournisseur.DateCreation = DateTime.UtcNow;
        fournisseur.DateModification = DateTime.UtcNow;

        context.Fournisseurs.Add(fournisseur);
        await context.SaveChangesAsync();

        logger.LogInformation("Fournisseur créé: {Societe} (Id={Id})", fournisseur.Societe, fournisseur.FournisseurId);
        return CreatedAtAction(nameof(GetFournisseur), new { id = fournisseur.FournisseurId }, mapper.Map<FournisseurDto>(fournisseur));
    }

    // PUT: api/Fournisseurs/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateFournisseur(int id, FournisseurDto dto)
    {
        var fournisseur = await context.Fournisseurs.FindAsync(id);
        if (fournisseur is null) return NotFound();

        mapper.Map(dto, fournisseur);
        fournisseur.FournisseurId = id; // guard against DTO overwrite
        fournisseur.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Fournisseur mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Fournisseurs/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFournisseur(int id)
    {
        var fournisseur = await context.Fournisseurs.FindAsync(id);
        if (fournisseur is null) return NotFound();

        context.Fournisseurs.Remove(fournisseur);
        await context.SaveChangesAsync();

        logger.LogInformation("Fournisseur supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves <see cref="FournisseurDto.AjouteParNom"/> and <see cref="FournisseurDto.ModifieParNom"/>
    /// from the Users table for all DTOs in a single batch query.
    /// </summary>
    private async Task ResolveUserNamesAsync(IList<FournisseurDto> dtos)
    {
        var ids = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0) return;

        var names = await context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, u => u.Name);

        foreach (var dto in dtos)
        {
            if (dto.AjoutePar.HasValue && names.TryGetValue(dto.AjoutePar.Value, out var nom))
                dto.AjouteParNom = nom;
            if (dto.ModifiePar.HasValue && names.TryGetValue(dto.ModifiePar.Value, out var modNom))
                dto.ModifieParNom = modNom;
        }
    }
}
