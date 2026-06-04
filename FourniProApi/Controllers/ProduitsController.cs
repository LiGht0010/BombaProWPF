using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProduitsController(AppDbContext context, IMapper mapper, ILogger<ProduitsController> logger)
    : ControllerBase
{
    // GET: api/Produits
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProduitDto>>> GetProduits()
    {
        var produits = await context.Produits.AsNoTracking().ToListAsync();
        var dtos     = mapper.Map<List<ProduitDto>>(produits);
        await ResolveUserNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Produits/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProduitDto>> GetProduit(int id)
    {
        var produit = await context.Produits.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProduitId == id);

        if (produit is null) return NotFound();

        var dto = mapper.Map<ProduitDto>(produit);
        await ResolveUserNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Produits
    [HttpPost]
    public async Task<ActionResult<ProduitDto>> CreateProduit(ProduitDto dto)
    {
        var produit = mapper.Map<Produit>(dto);
        produit.DateCreation = DateTime.UtcNow;
        produit.DateModification = DateTime.UtcNow;

        // Ensure PrixTTC is consistent
        if (produit.PrixTTC is null)
            produit.CalculatePrixTTC();

        context.Produits.Add(produit);
        await context.SaveChangesAsync();

        logger.LogInformation("Produit créé: {NumeroProduit} (Id={Id})", produit.NumeroProduit, produit.ProduitId);
        return CreatedAtAction(nameof(GetProduit), new { id = produit.ProduitId }, mapper.Map<ProduitDto>(produit));
    }

    // PUT: api/Produits/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduit(int id, ProduitDto dto)
    {
        var produit = await context.Produits.FindAsync(id);
        if (produit is null) return NotFound();

        mapper.Map(dto, produit);
        produit.ProduitId = id; // guard against DTO overwrite
        produit.DateModification = DateTime.UtcNow;

        // Recalculate TTC if HT or TVA changed
        produit.CalculatePrixTTC();

        await context.SaveChangesAsync();

        logger.LogInformation("Produit mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Produits/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduit(int id)
    {
        var produit = await context.Produits.FindAsync(id);
        if (produit is null) return NotFound();

        context.Produits.Remove(produit);
        await context.SaveChangesAsync();

        logger.LogInformation("Produit supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves <see cref="ProduitDto.AjouteParNom"/> and <see cref="ProduitDto.ModifieParNom"/>
    /// from the Users table for all DTOs in a single batch query.
    /// </summary>
    private async Task ResolveUserNamesAsync(IList<ProduitDto> dtos)
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
