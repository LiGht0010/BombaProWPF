using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockVoyagesController(AppDbContext context, IMapper mapper, ILogger<StockVoyagesController> logger)
    : ControllerBase
{
    // GET: api/StockVoyages?voyageId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockVoyageDto>>> GetStocks([FromQuery] int? voyageId)
    {
        var query = context.StockVoyages.AsNoTracking();
        if (voyageId.HasValue)
            query = query.Where(s => s.VoyageId == voyageId.Value);

        var stocks = await query.ToListAsync();
        var dtos = mapper.Map<List<StockVoyageDto>>(stocks);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/StockVoyages/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StockVoyageDto>> GetStock(int id)
    {
        var stock = await context.StockVoyages.AsNoTracking()
            .FirstOrDefaultAsync(s => s.StockVoyageId == id);

        if (stock is null) return NotFound();

        var dto = mapper.Map<StockVoyageDto>(stock);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/StockVoyages
    [HttpPost]
    public async Task<ActionResult<StockVoyageDto>> CreateStock(StockVoyageDto dto)
    {
        var stock = mapper.Map<StockVoyage>(dto);
        stock.StockVoyageId = 0;

        context.StockVoyages.Add(stock);
        await context.SaveChangesAsync();

        logger.LogInformation("StockVoyage créé: Id={Id}", stock.StockVoyageId);
        return CreatedAtAction(nameof(GetStock), new { id = stock.StockVoyageId }, mapper.Map<StockVoyageDto>(stock));
    }

    // PUT: api/StockVoyages/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStock(int id, StockVoyageDto dto)
    {
        var stock = await context.StockVoyages.FindAsync(id);
        if (stock is null) return NotFound();

        mapper.Map(dto, stock);
        stock.StockVoyageId = id;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/StockVoyages/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStock(int id)
    {
        var stock = await context.StockVoyages.FindAsync(id);
        if (stock is null) return NotFound();

        context.StockVoyages.Remove(stock);
        await context.SaveChangesAsync();

        logger.LogInformation("StockVoyage supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<StockVoyageDto> dtos)
    {
        var produitIds = dtos.Where(d => d.ProduitId.HasValue)
            .Select(d => d.ProduitId!.Value).Distinct().ToList();

        if (produitIds.Count == 0) return;

        var produitNames = await context.Produits.AsNoTracking()
            .Where(p => produitIds.Contains(p.ProduitId))
            .ToDictionaryAsync(p => p.ProduitId,
                p => !string.IsNullOrWhiteSpace(p.Description) ? p.Description : p.NumeroProduit);

        foreach (var dto in dtos)
            if (dto.ProduitId.HasValue && produitNames.TryGetValue(dto.ProduitId.Value, out var nom))
                dto.ProduitNom = nom;
    }
}
