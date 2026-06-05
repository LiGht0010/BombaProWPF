using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployesController(AppDbContext context, IMapper mapper, ILogger<EmployesController> logger)
    : ControllerBase
{
    // GET: api/Employes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeDto>>> GetEmployes()
    {
        var employes = await context.Employes
            .AsNoTracking()
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .ToListAsync();

        var dtos = mapper.Map<List<EmployeDto>>(employes);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Employes/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeDto>> GetEmploye(int id)
    {
        var employe = await context.Employes
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeId == id);

        if (employe is null) return NotFound();

        var dto = mapper.Map<EmployeDto>(employe);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Employes
    [HttpPost]
    public async Task<ActionResult<EmployeDto>> CreateEmploye(EmployeDto dto)
    {
        var employe = mapper.Map<Employe>(dto);
        employe.EmployeId        = 0;
        employe.DateCreation     = DateTime.UtcNow;
        employe.DateModification = DateTime.UtcNow;

        context.Employes.Add(employe);
        await context.SaveChangesAsync();

        logger.LogInformation("Employé créé: {Nom} {Prenom} (Id={Id})", employe.Nom, employe.Prenom, employe.EmployeId);
        return CreatedAtAction(nameof(GetEmploye), new { id = employe.EmployeId }, mapper.Map<EmployeDto>(employe));
    }

    // PUT: api/Employes/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmploye(int id, EmployeDto dto)
    {
        var employe = await context.Employes.FindAsync(id);
        if (employe is null) return NotFound();

        mapper.Map(dto, employe);
        employe.EmployeId        = id;
        employe.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Employé mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Employes/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmploye(int id)
    {
        var employe = await context.Employes.FindAsync(id);
        if (employe is null) return NotFound();

        context.Employes.Remove(employe);
        await context.SaveChangesAsync();

        logger.LogInformation("Employé supprimé: Id={Id}", id);
        return NoContent();
    }

    // GET: api/Employes/{id}/hasrelatedrecords
    [HttpGet("{id:int}/hasrelatedrecords")]
    public async Task<ActionResult<bool>> HasRelatedRecords(int id)
    {
        var hasVentes = await context.Ventes
            .AsNoTracking()
            .AnyAsync(v => v.EmployeId == id);

        var hasAchats = await context.Achats
            .AsNoTracking()
            .AnyAsync(a => a.EmployeId == id);

        return Ok(hasVentes || hasAchats);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<EmployeDto> dtos)
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
