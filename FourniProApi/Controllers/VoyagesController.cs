using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoyagesController(AppDbContext context, IMapper mapper, ILogger<VoyagesController> logger)
    : ControllerBase
{
    // GET: api/Voyages
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VoyageDto>>> GetVoyages()
    {
        var voyages = await context.Voyages.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<VoyageDto>>(voyages);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Voyages/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VoyageDto>> GetVoyage(int id)
    {
        var voyage = await context.Voyages.AsNoTracking()
            .FirstOrDefaultAsync(v => v.VoyageId == id);

        if (voyage is null) return NotFound();

        var dto = mapper.Map<VoyageDto>(voyage);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Voyages
    [HttpPost]
    public async Task<ActionResult<VoyageDto>> CreateVoyage(VoyageDto dto)
    {
        var voyage = mapper.Map<Voyage>(dto);
        voyage.VoyageId = 0;
        voyage.DateCreation    = DateTime.UtcNow;
        voyage.DateModification = DateTime.UtcNow;
        if (voyage.DateDepart.HasValue)
            voyage.DateDepart = DateTime.SpecifyKind(voyage.DateDepart.Value, DateTimeKind.Utc);

        context.Voyages.Add(voyage);
        await context.SaveChangesAsync();

        logger.LogInformation("Voyage créé: Id={Id}", voyage.VoyageId);
        return CreatedAtAction(nameof(GetVoyage), new { id = voyage.VoyageId }, mapper.Map<VoyageDto>(voyage));
    }

    // PUT: api/Voyages/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVoyage(int id, VoyageDto dto)
    {
        var voyage = await context.Voyages.FindAsync(id);
        if (voyage is null) return NotFound();

        mapper.Map(dto, voyage);
        voyage.VoyageId = id;
        voyage.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Voyage mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Voyages/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVoyage(int id)
    {
        var voyage = await context.Voyages.FindAsync(id);
        if (voyage is null) return NotFound();

        context.Voyages.Remove(voyage);
        await context.SaveChangesAsync();

        logger.LogInformation("Voyage supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<VoyageDto> dtos)
    {
        var camionIds = dtos.Where(d => d.CamionId.HasValue).Select(d => d.CamionId!.Value).Distinct().ToList();
        var chauffeurIds = dtos.Where(d => d.ChauffeurId.HasValue).Select(d => d.ChauffeurId!.Value).Distinct().ToList();
        var citerneIds = dtos.Where(d => d.CiterneId.HasValue).Select(d => d.CiterneId!.Value).Distinct().ToList();
        var userIds = dtos.SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

        var camionNames = camionIds.Count > 0
            ? await context.Camions.AsNoTracking()
                .Where(c => camionIds.Contains(c.CamionId))
                .ToDictionaryAsync(c => c.CamionId, c => c.Matricule ?? string.Empty)
            : [];

        var chauffeurNames = chauffeurIds.Count > 0
            ? await context.Chauffeurs.AsNoTracking()
                .Where(c => chauffeurIds.Contains(c.ChauffeurId))
                .ToDictionaryAsync(c => c.ChauffeurId, c => $"{c.Prenom} {c.Nom}")
            : [];

        var citerneNames = citerneIds.Count > 0
            ? await context.Citernes.AsNoTracking()
                .Where(c => citerneIds.Contains(c.CiterneId))
                .ToDictionaryAsync(c => c.CiterneId, c => c.MatriculeCiterne ?? string.Empty)
            : [];

        var userNames = userIds.Count > 0
            ? await context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.Name)
            : [];

        foreach (var dto in dtos)
        {
            if (dto.CamionId.HasValue && camionNames.TryGetValue(dto.CamionId.Value, out var cm))
                dto.CamionMatricule = cm;
            if (dto.ChauffeurId.HasValue && chauffeurNames.TryGetValue(dto.ChauffeurId.Value, out var ch))
                dto.ChauffeurNom = ch;
            if (dto.CiterneId.HasValue && citerneNames.TryGetValue(dto.CiterneId.Value, out var ci))
                dto.CiterneMatricule = ci;
            if (dto.AjoutePar.HasValue && userNames.TryGetValue(dto.AjoutePar.Value, out var ajNom))
                dto.AjouteParNom = ajNom;
            if (dto.ModifiePar.HasValue && userNames.TryGetValue(dto.ModifiePar.Value, out var moNom))
                dto.ModifieParNom = moNom;
        }
    }
}
