using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FraisVoyagesController(AppDbContext context, IMapper mapper, ILogger<FraisVoyagesController> logger)
    : ControllerBase
{
    // GET: api/FraisVoyages?voyageId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FraisVoyageDto>>> GetFrais([FromQuery] int? voyageId)
    {
        var query = context.FraisVoyages.AsNoTracking();
        if (voyageId.HasValue)
            query = query.Where(f => f.VoyageId == voyageId.Value);

        var frais = await query.ToListAsync();
        return Ok(mapper.Map<List<FraisVoyageDto>>(frais));
    }

    // GET: api/FraisVoyages/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FraisVoyageDto>> GetFrai(int id)
    {
        var frai = await context.FraisVoyages.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FraisVoyageId == id);

        if (frai is null) return NotFound();
        return Ok(mapper.Map<FraisVoyageDto>(frai));
    }

    // POST: api/FraisVoyages
    [HttpPost]
    public async Task<ActionResult<FraisVoyageDto>> CreateFrai(FraisVoyageDto dto)
    {
        var frai = mapper.Map<FraisVoyage>(dto);
        frai.FraisVoyageId = 0;

        context.FraisVoyages.Add(frai);
        await context.SaveChangesAsync();

        logger.LogInformation("FraisVoyage créé: Id={Id}", frai.FraisVoyageId);
        return CreatedAtAction(nameof(GetFrai), new { id = frai.FraisVoyageId }, mapper.Map<FraisVoyageDto>(frai));
    }

    // PUT: api/FraisVoyages/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateFrai(int id, FraisVoyageDto dto)
    {
        var frai = await context.FraisVoyages.FindAsync(id);
        if (frai is null) return NotFound();

        mapper.Map(dto, frai);
        frai.FraisVoyageId = id;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/FraisVoyages/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFrai(int id)
    {
        var frai = await context.FraisVoyages.FindAsync(id);
        if (frai is null) return NotFound();

        context.FraisVoyages.Remove(frai);
        await context.SaveChangesAsync();

        logger.LogInformation("FraisVoyage supprimé: Id={Id}", id);
        return NoContent();
    }
}
