using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BonLivraisonDetailsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public BonLivraisonDetailsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/BonLivraisonDetails
    [HttpGet]
    public async Task<ActionResult<List<BonLivraisonDetailsDto>>> GetBonLivraisonDetails()
    {
        var details = await _context.BonLivraisonDetails
            .Include(d => d.Produit)
            .Include(d => d.Service)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDetailsDto>>(details);
        return Ok(dtos);
    }

    // GET: api/BonLivraisonDetails/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BonLivraisonDetailsDto>> GetBonLivraisonDetails(int id)
    {
        var detail = await _context.BonLivraisonDetails
            .Include(d => d.Produit)
            .Include(d => d.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ID == id);

        if (detail == null)
            return NotFound();

        var dto = _mapper.Map<BonLivraisonDetailsDto>(detail);
        return Ok(dto);
    }

    // GET: api/BonLivraisonDetails/bonlivraison/5
    [HttpGet("bonlivraison/{bonLivraisonId}")]
    public async Task<ActionResult<List<BonLivraisonDetailsDto>>> GetDetailsByBonLivraison(int bonLivraisonId)
    {
        var details = await _context.BonLivraisonDetails
            .Include(d => d.Produit)
            .Include(d => d.Service)
            .Where(d => d.BonLivraisonID == bonLivraisonId)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDetailsDto>>(details);
        return Ok(dtos);
    }

    // GET: api/BonLivraisonDetails/produit/5
    [HttpGet("produit/{produitId}")]
    public async Task<ActionResult<List<BonLivraisonDetailsDto>>> GetDetailsByProduit(int produitId)
    {
        var details = await _context.BonLivraisonDetails
            .Include(d => d.Produit)
            .Include(d => d.Service)
            .Include(d => d.BonLivraison)
            .Where(d => d.ProduitID == produitId)
            .AsNoTracking()
            .OrderByDescending(d => d.BonLivraison.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDetailsDto>>(details);
        return Ok(dtos);
    }

    // GET: api/BonLivraisonDetails/service/5
    [HttpGet("service/{serviceId}")]
    public async Task<ActionResult<List<BonLivraisonDetailsDto>>> GetDetailsByService(int serviceId)
    {
        var details = await _context.BonLivraisonDetails
            .Include(d => d.Produit)
            .Include(d => d.Service)
            .Include(d => d.BonLivraison)
            .Where(d => d.ServiceID == serviceId)
            .AsNoTracking()
            .OrderByDescending(d => d.BonLivraison.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDetailsDto>>(details);
        return Ok(dtos);
    }

    // PUT: api/BonLivraisonDetails/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBonLivraisonDetails(int id, [FromBody] BonLivraisonDetailsDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch.");

        var existing = await _context.BonLivraisonDetails
            .Include(d => d.BonLivraison)
            .FirstOrDefaultAsync(d => d.ID == id);

        if (existing == null)
            return NotFound();

        // Prevent modification if parent BL is invoiced
        if (existing.BonLivraison.EstFacture)
        {
            return BadRequest("Impossible de modifier les détails d'un bon de livraison déjà facturé.");
        }

        // Update properties
        existing.ProduitID = dto.ProduitID;
        existing.ServiceID = dto.ServiceID;
        existing.Quantite = dto.Quantite;
        existing.PrixUnitaire = dto.PrixUnitaire;
        existing.Description = dto.Description;
        existing.MontantLigne = dto.Quantite * dto.PrixUnitaire;

        // Recalculate parent BL total
        await RecalculateBonLivraisonTotalAsync(existing.BonLivraisonID);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BonLivraisonDetailsExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // POST: api/BonLivraisonDetails
    [HttpPost]
    public async Task<ActionResult<BonLivraisonDetailsDto>> PostBonLivraisonDetails([FromBody] BonLivraisonDetailsDto dto)
    {
        try
        {
            // Validate parent BL exists and is not invoiced
            var bonLivraison = await _context.BonsLivraison.FindAsync(dto.BonLivraisonID);
            if (bonLivraison == null)
            {
                return BadRequest($"Le bon de livraison avec l'ID {dto.BonLivraisonID} n'existe pas.");
            }

            if (bonLivraison.EstFacture)
            {
                return BadRequest("Impossible d'ajouter des détails à un bon de livraison déjà facturé.");
            }

            var entity = new BonLivraisonDetails
            {
                BonLivraisonID = dto.BonLivraisonID,
                ProduitID = dto.ProduitID,
                ServiceID = dto.ServiceID,
                Quantite = dto.Quantite,
                PrixUnitaire = dto.PrixUnitaire,
                Description = dto.Description,
                MontantLigne = dto.Quantite * dto.PrixUnitaire
            };

            _context.BonLivraisonDetails.Add(entity);
            
            // Recalculate parent BL total
            await RecalculateBonLivraisonTotalAsync(dto.BonLivraisonID);
            
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdEntity = await _context.BonLivraisonDetails
                .Include(d => d.Produit)
                .Include(d => d.Service)
                .FirstOrDefaultAsync(d => d.ID == entity.ID);

            var resultDto = _mapper.Map<BonLivraisonDetailsDto>(createdEntity);
            return CreatedAtAction(nameof(GetBonLivraisonDetails), new { id = entity.ID }, resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // POST: api/BonLivraisonDetails/batch
    [HttpPost("batch")]
    public async Task<ActionResult<List<BonLivraisonDetailsDto>>> PostBonLivraisonDetailsBatch([FromBody] List<BonLivraisonDetailsDto> dtos)
    {
        try
        {
            if (!dtos.Any())
                return BadRequest("La liste des détails ne peut pas être vide.");

            // Validate all belong to same BL and BL is not invoiced
            var bonLivraisonId = dtos.First().BonLivraisonID;
            if (dtos.Any(d => d.BonLivraisonID != bonLivraisonId))
            {
                return BadRequest("Tous les détails doivent appartenir au même bon de livraison.");
            }

            var bonLivraison = await _context.BonsLivraison.FindAsync(bonLivraisonId);
            if (bonLivraison == null)
            {
                return BadRequest($"Le bon de livraison avec l'ID {bonLivraisonId} n'existe pas.");
            }

            if (bonLivraison.EstFacture)
            {
                return BadRequest("Impossible d'ajouter des détails à un bon de livraison déjà facturé.");
            }

            var entities = new List<BonLivraisonDetails>();

            foreach (var dto in dtos)
            {
                var entity = new BonLivraisonDetails
                {
                    BonLivraisonID = dto.BonLivraisonID,
                    ProduitID = dto.ProduitID,
                    ServiceID = dto.ServiceID,
                    Quantite = dto.Quantite,
                    PrixUnitaire = dto.PrixUnitaire,
                    Description = dto.Description,
                    MontantLigne = dto.Quantite * dto.PrixUnitaire
                };

                entities.Add(entity);
            }

            _context.BonLivraisonDetails.AddRange(entities);
            
            // Recalculate parent BL total
            await RecalculateBonLivraisonTotalAsync(bonLivraisonId);
            
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var ids = entities.Select(e => e.ID).ToList();
            var createdEntities = await _context.BonLivraisonDetails
                .Include(d => d.Produit)
                .Include(d => d.Service)
                .Where(d => ids.Contains(d.ID))
                .ToListAsync();

            var resultDtos = _mapper.Map<List<BonLivraisonDetailsDto>>(createdEntities);
            return Ok(resultDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // DELETE: api/BonLivraisonDetails/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBonLivraisonDetails(int id)
    {
        var detail = await _context.BonLivraisonDetails
            .Include(d => d.BonLivraison)
            .FirstOrDefaultAsync(d => d.ID == id);

        if (detail == null)
            return NotFound();

        // Prevent deletion if parent BL is invoiced
        if (detail.BonLivraison.EstFacture)
        {
            return BadRequest("Impossible de supprimer les détails d'un bon de livraison déjà facturé.");
        }

        var bonLivraisonId = detail.BonLivraisonID;

        _context.BonLivraisonDetails.Remove(detail);
        
        // Recalculate parent BL total
        await RecalculateBonLivraisonTotalAsync(bonLivraisonId);
        
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/BonLivraisonDetails/bonlivraison/5
    [HttpDelete("bonlivraison/{bonLivraisonId}")]
    public async Task<IActionResult> DeleteDetailsByBonLivraison(int bonLivraisonId)
    {
        var bonLivraison = await _context.BonsLivraison.FindAsync(bonLivraisonId);
        if (bonLivraison == null)
            return NotFound();

        if (bonLivraison.EstFacture)
        {
            return BadRequest("Impossible de supprimer les détails d'un bon de livraison déjà facturé.");
        }

        var details = await _context.BonLivraisonDetails
            .Where(d => d.BonLivraisonID == bonLivraisonId)
            .ToListAsync();

        if (!details.Any())
            return NotFound();

        _context.BonLivraisonDetails.RemoveRange(details);
        
        // Reset parent BL total to 0
        bonLivraison.MontantTotal = 0;
        
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BonLivraisonDetailsExists(int id)
    {
        return _context.BonLivraisonDetails.Any(e => e.ID == id);
    }

    private async Task RecalculateBonLivraisonTotalAsync(int bonLivraisonId)
    {
        var bonLivraison = await _context.BonsLivraison
            .Include(bl => bl.Details)
            .FirstOrDefaultAsync(bl => bl.ID == bonLivraisonId);

        if (bonLivraison != null)
        {
            bonLivraison.MontantTotal = bonLivraison.Details.Sum(d => d.MontantLigne);
        }
    }
}
