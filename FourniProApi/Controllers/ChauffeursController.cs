using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChauffeursController(AppDbContext context, IMapper mapper, ILogger<ChauffeursController> logger)
    : ControllerBase
{
    // GET: api/Chauffeurs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChauffeurDto>>> GetChauffeurs()
    {
        var chauffeurs = await context.Chauffeurs.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<ChauffeurDto>>(chauffeurs);
        await ResolveUserNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Chauffeurs/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChauffeurDto>> GetChauffeur(int id)
    {
        var chauffeur = await context.Chauffeurs.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ChauffeurId == id);

        if (chauffeur is null) return NotFound();

        var dto = mapper.Map<ChauffeurDto>(chauffeur);
        await ResolveUserNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Chauffeurs
    [HttpPost]
    public async Task<ActionResult<ChauffeurDto>> CreateChauffeur(ChauffeurDto dto)
    {
        var chauffeur = mapper.Map<Chauffeur>(dto);
        chauffeur.DateCreation = DateTime.UtcNow;
        chauffeur.DateModification = DateTime.UtcNow;

        context.Chauffeurs.Add(chauffeur);
        await context.SaveChangesAsync();

        logger.LogInformation("Chauffeur créé: {Nom} (Id={Id})", chauffeur.Nom, chauffeur.ChauffeurId);
        return CreatedAtAction(nameof(GetChauffeur), new { id = chauffeur.ChauffeurId }, mapper.Map<ChauffeurDto>(chauffeur));
    }

    // PUT: api/Chauffeurs/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateChauffeur(int id, ChauffeurDto dto)
    {
        var chauffeur = await context.Chauffeurs.FindAsync(id);
        if (chauffeur is null) return NotFound();

        mapper.Map(dto, chauffeur);
        chauffeur.ChauffeurId = id;
        chauffeur.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Chauffeur mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Chauffeurs/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteChauffeur(int id)
    {
        var chauffeur = await context.Chauffeurs.FindAsync(id);
        if (chauffeur is null) return NotFound();

        context.Chauffeurs.Remove(chauffeur);
        await context.SaveChangesAsync();

        logger.LogInformation("Chauffeur supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ResolveUserNamesAsync(IList<ChauffeurDto> dtos)
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
