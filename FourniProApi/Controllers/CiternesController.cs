using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CiternesController(AppDbContext context, IMapper mapper, ILogger<CiternesController> logger)
    : ControllerBase
{
    // GET: api/Citernes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CiterneDto>>> GetCiternes()
    {
        var citernes = await context.Citernes.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<CiterneDto>>(citernes);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Citernes/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CiterneDto>> GetCiterne(int id)
    {
        var citerne = await context.Citernes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CiterneId == id);

        if (citerne is null) return NotFound();

        var dto = mapper.Map<CiterneDto>(citerne);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Citernes
    [HttpPost]
    public async Task<ActionResult<CiterneDto>> CreateCiterne(CiterneDto dto)
    {
        var citerne = mapper.Map<Citerne>(dto);
        citerne.CiterneId = 0;
        citerne.DateCreation = DateTime.UtcNow;
        citerne.DateModification = DateTime.UtcNow;

        context.Citernes.Add(citerne);
        await context.SaveChangesAsync();

        logger.LogInformation("Citerne créée: {Matricule} (Id={Id})", citerne.MatriculeCiterne, citerne.CiterneId);
        return CreatedAtAction(nameof(GetCiterne), new { id = citerne.CiterneId }, mapper.Map<CiterneDto>(citerne));
    }

    // PUT: api/Citernes/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCiterne(int id, CiterneDto dto)
    {
        var citerne = await context.Citernes.FindAsync(id);
        if (citerne is null) return NotFound();

        mapper.Map(dto, citerne);
        citerne.CiterneId = id;
        citerne.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Citerne modifiée: {Matricule} (Id={Id})", citerne.MatriculeCiterne, citerne.CiterneId);
        return NoContent();
    }

    // DELETE: api/Citernes/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCiterne(int id)
    {
        var citerne = await context.Citernes.FindAsync(id);
        if (citerne is null) return NotFound();

        context.Citernes.Remove(citerne);
        await context.SaveChangesAsync();

        logger.LogInformation("Citerne supprimée: Id={Id}", id);
        return NoContent();
    }

    private async Task ResolveNamesAsync(IEnumerable<CiterneDto> dtos)
    {
        var ids = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0) return;

        var users = await context.Users
            .Where(u => ids.Contains(u.UserId))
            .Select(u => new { u.UserId, u.Name })
            .ToDictionaryAsync(u => u.UserId, u => u.Name);

        foreach (var dto in dtos)
        {
            if (dto.AjoutePar.HasValue && users.TryGetValue(dto.AjoutePar.Value, out var n1))
                dto.AjouteParNom = n1;
            if (dto.ModifiePar.HasValue && users.TryGetValue(dto.ModifiePar.Value, out var n2))
                dto.ModifieParNom = n2;
        }
    }
}
