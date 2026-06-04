using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CamionsController(AppDbContext context, IMapper mapper, ILogger<CamionsController> logger)
    : ControllerBase
{
    // GET: api/Camions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CamionDto>>> GetCamions()
    {
        var camions = await context.Camions.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<CamionDto>>(camions);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Camions/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CamionDto>> GetCamion(int id)
    {
        var camion = await context.Camions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CamionId == id);

        if (camion is null) return NotFound();

        var dto = mapper.Map<CamionDto>(camion);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Camions
    [HttpPost]
    public async Task<ActionResult<CamionDto>> CreateCamion(CamionDto dto)
    {
        var camion = mapper.Map<Camion>(dto);
        camion.CamionId = 0; // let the DB generate the identity
        camion.DateCreation = DateTime.UtcNow;
        camion.DateModification = DateTime.UtcNow;

        context.Camions.Add(camion);
        await context.SaveChangesAsync();

        logger.LogInformation("Camion créé: {Matricule} (Id={Id})", camion.Matricule, camion.CamionId);
        return CreatedAtAction(nameof(GetCamion), new { id = camion.CamionId }, mapper.Map<CamionDto>(camion));
    }

    // PUT: api/Camions/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCamion(int id, CamionDto dto)
    {
        var camion = await context.Camions.FindAsync(id);
        if (camion is null) return NotFound();

        mapper.Map(dto, camion);
        camion.CamionId = id;
        camion.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Camion mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Camions/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCamion(int id)
    {
        var camion = await context.Camions.FindAsync(id);
        if (camion is null) return NotFound();

        context.Camions.Remove(camion);
        await context.SaveChangesAsync();

        logger.LogInformation("Camion supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<CamionDto> dtos)
    {
        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0) return;

        var userNames = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, u => u.Name);

        foreach (var dto in dtos)
        {
            if (dto.AjoutePar.HasValue && userNames.TryGetValue(dto.AjoutePar.Value, out var ajouteNom))
                dto.AjouteParNom = ajouteNom;
            if (dto.ModifiePar.HasValue && userNames.TryGetValue(dto.ModifiePar.Value, out var modNom))
                dto.ModifieParNom = modNom;
        }
    }
}
