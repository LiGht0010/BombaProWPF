using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ElementsFactureController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ElementsFactureController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/ElementsFacture
    [HttpGet]
    public async Task<ActionResult<List<ElementsFactureDto>>> GetElementsFacture()
    {
        var elements = await _context.ElementsFactures
            .Include(e => e.Facture)
            .Include(e => e.Produit)
            .Include(e => e.Service)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<ElementsFactureDto>>(elements);
        return Ok(dtos);
    }

    // GET: api/ElementsFacture/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ElementsFactureDto>> GetElementFacture(int id)
    {
        var element = await _context.ElementsFactures
            .Include(e => e.Facture)
            .Include(e => e.Produit)
            .Include(e => e.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ID == id);

        if (element == null)
            return NotFound();

        var dto = _mapper.Map<ElementsFactureDto>(element);
        return Ok(dto);
    }

    // GET: api/ElementsFacture/facture/5
    [HttpGet("facture/{factureId}")]
    public async Task<ActionResult<List<ElementsFactureDto>>> GetElementsByFacture(int factureId)
    {
        var elements = await _context.ElementsFactures
            .Include(e => e.Facture)
            .Include(e => e.Produit)
            .Include(e => e.Service)
            .Where(e => e.FactureID == factureId)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<ElementsFactureDto>>(elements);
        return Ok(dtos);
    }

    // GET: api/ElementsFacture/produit/5
    [HttpGet("produit/{produitId}")]
    public async Task<ActionResult<List<ElementsFactureDto>>> GetElementsByProduit(int produitId)
    {
        var elements = await _context.ElementsFactures
            .Include(e => e.Facture)
            .Include(e => e.Produit)
            .Include(e => e.Service)
            .Where(e => e.ProduitID == produitId)
            .AsNoTracking()
            .OrderByDescending(e => e.Facture != null ? e.Facture.DateFacture : null)
            .ToListAsync();

        var dtos = _mapper.Map<List<ElementsFactureDto>>(elements);
        return Ok(dtos);
    }

    // GET: api/ElementsFacture/service/5
    [HttpGet("service/{serviceId}")]
    public async Task<ActionResult<List<ElementsFactureDto>>> GetElementsByService(int serviceId)
    {
        var elements = await _context.ElementsFactures
            .Include(e => e.Facture)
            .Include(e => e.Produit)
            .Include(e => e.Service)
            .Where(e => e.ServiceID == serviceId)
            .AsNoTracking()
            .OrderByDescending(e => e.Facture != null ? e.Facture.DateFacture : null)
            .ToListAsync();

        var dtos = _mapper.Map<List<ElementsFactureDto>>(elements);
        return Ok(dtos);
    }

    // PUT: api/ElementsFacture/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutElementFacture(int id, [FromBody] ElementsFactureDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch.");

        var existing = await _context.ElementsFactures.FindAsync(id);
        if (existing == null)
            return NotFound();

        // Update properties
        existing.FactureID = dto.FactureID;
        existing.ProduitID = dto.ProduitID;
        existing.ServiceID = dto.ServiceID;
        existing.Quantite = dto.Quantite;
        existing.PrixUnitaire = dto.PrixUnitaire;

        // Recalculate facture total
        if (existing.FactureID.HasValue)
        {
            await RecalculateFactureTotalAsync(existing.FactureID.Value);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ElementFactureExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // POST: api/ElementsFacture
    [HttpPost]
    public async Task<ActionResult<ElementsFactureDto>> PostElementFacture([FromBody] ElementsFactureDto dto)
    {
        try
        {
            // Validate facture exists
            if (dto.FactureID.HasValue && !await _context.Factures.AnyAsync(f => f.ID == dto.FactureID))
            {
                return BadRequest($"La facture avec l'ID {dto.FactureID} n'existe pas.");
            }

            var entity = new ElementsFacture
            {
                FactureID = dto.FactureID,
                ProduitID = dto.ProduitID,
                ServiceID = dto.ServiceID,
                Quantite = dto.Quantite,
                PrixUnitaire = dto.PrixUnitaire
            };

            _context.ElementsFactures.Add(entity);

            // Recalculate facture total
            if (dto.FactureID.HasValue)
            {
                await RecalculateFactureTotalAsync(dto.FactureID.Value);
            }

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdEntity = await _context.ElementsFactures
                .Include(e => e.Facture)
                .Include(e => e.Produit)
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.ID == entity.ID);

            var resultDto = _mapper.Map<ElementsFactureDto>(createdEntity);
            return CreatedAtAction(nameof(GetElementFacture), new { id = entity.ID }, resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // POST: api/ElementsFacture/batch
    [HttpPost("batch")]
    public async Task<ActionResult<List<ElementsFactureDto>>> PostElementsFactureBatch([FromBody] List<ElementsFactureDto> dtos)
    {
        try
        {
            if (!dtos.Any())
                return BadRequest("La liste des éléments ne peut pas être vide.");

            var entities = new List<ElementsFacture>();

            foreach (var dto in dtos)
            {
                var entity = new ElementsFacture
                {
                    FactureID = dto.FactureID,
                    ProduitID = dto.ProduitID,
                    ServiceID = dto.ServiceID,
                    Quantite = dto.Quantite,
                    PrixUnitaire = dto.PrixUnitaire
                };

                entities.Add(entity);
            }

            _context.ElementsFactures.AddRange(entities);

            // Recalculate facture totals for all affected factures
            var factureIds = dtos.Where(d => d.FactureID.HasValue).Select(d => d.FactureID!.Value).Distinct();
            foreach (var factureId in factureIds)
            {
                await RecalculateFactureTotalAsync(factureId);
            }

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var ids = entities.Select(e => e.ID).ToList();
            var createdEntities = await _context.ElementsFactures
                .Include(e => e.Facture)
                .Include(e => e.Produit)
                .Include(e => e.Service)
                .Where(e => ids.Contains(e.ID))
                .ToListAsync();

            var resultDtos = _mapper.Map<List<ElementsFactureDto>>(createdEntities);
            return Ok(resultDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // DELETE: api/ElementsFacture/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteElementFacture(int id)
    {
        var element = await _context.ElementsFactures.FindAsync(id);
        if (element == null)
            return NotFound();

        var factureId = element.FactureID;

        _context.ElementsFactures.Remove(element);

        // Recalculate facture total
        if (factureId.HasValue)
        {
            await RecalculateFactureTotalAsync(factureId.Value);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/ElementsFacture/facture/5
    [HttpDelete("facture/{factureId}")]
    public async Task<IActionResult> DeleteElementsByFacture(int factureId)
    {
        var elements = await _context.ElementsFactures
            .Where(e => e.FactureID == factureId)
            .ToListAsync();

        if (!elements.Any())
            return NotFound();

        _context.ElementsFactures.RemoveRange(elements);

        // Reset facture total to 0
        var facture = await _context.Factures.FindAsync(factureId);
        if (facture != null)
        {
            facture.MontantTotal = 0;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ElementFactureExists(int id)
    {
        return _context.ElementsFactures.Any(e => e.ID == id);
    }

    private async Task RecalculateFactureTotalAsync(int factureId)
    {
        var facture = await _context.Factures
            .Include(f => f.ElementsFactures)
            .FirstOrDefaultAsync(f => f.ID == factureId);

        if (facture != null)
        {
            facture.MontantTotal = facture.ElementsFactures
                .Where(e => e.Quantite.HasValue && e.PrixUnitaire.HasValue)
                .Sum(e => e.Quantite!.Value * e.PrixUnitaire!.Value);
        }
    }
}
