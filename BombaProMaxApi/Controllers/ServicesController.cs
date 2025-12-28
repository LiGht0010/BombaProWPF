using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ServicesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Services
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServices()
    {
        var services = await _context.Services
            .Include(s => s.ServiceCategorie)
            .OrderBy(s => s.Numero)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ServiceDto>>(services));
    }

    // GET: api/Services/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceDto>> GetService(int id)
    {
        var service = await _context.Services
            .Include(s => s.ServiceCategorie)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ID == id);

        if (service == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<ServiceDto>(service));
    }

    // GET: api/Services/category/5
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServicesByCategory(int categoryId)
    {
        var services = await _context.Services
            .Include(s => s.ServiceCategorie)
            .Where(s => s.ServiceCategorieID == categoryId)
            .OrderBy(s => s.Numero)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ServiceDto>>(services));
    }

    // GET: api/Services/search?term=lavage
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> SearchServices([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return await GetServices();
        }

        var searchTerm = term.ToLower();
        var services = await _context.Services
            .Include(s => s.ServiceCategorie)
            .Where(s => 
                (s.Numero != null && s.Numero.ToLower().Contains(searchTerm)) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm)) ||
                (s.ServiceCategorie != null && s.ServiceCategorie.Nom.ToLower().Contains(searchTerm)))
            .OrderBy(s => s.Numero)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ServiceDto>>(services));
    }

    // PUT: api/Services/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutService(int id, ServiceDto dto)
    {
        if (id != dto.ID)
        {
            return BadRequest("ID mismatch.");
        }

        var existing = await _context.Services.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Validate category if provided
        if (dto.ServiceCategorieID.HasValue)
        {
            var categoryExists = await _context.ServiceCategories
                .AnyAsync(c => c.ID == dto.ServiceCategorieID.Value && c.IsActive);
            if (!categoryExists)
            {
                return BadRequest("La catégorie spécifiée n'existe pas ou n'est pas active.");
            }
        }

        existing.Numero = dto.Numero;
        existing.Description = dto.Description;
        existing.Prix = dto.Prix;
        existing.ServiceCategorieID = dto.ServiceCategorieID;
        existing.ModifiePar = dto.ModifiePar;
        existing.DateModification = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // POST: api/Services
    [HttpPost]
    public async Task<ActionResult<ServiceDto>> PostService(ServiceDto dto)
    {
        // Validate category if provided
        if (dto.ServiceCategorieID.HasValue)
        {
            var categoryExists = await _context.ServiceCategories
                .AnyAsync(c => c.ID == dto.ServiceCategorieID.Value && c.IsActive);
            if (!categoryExists)
            {
                return BadRequest("La catégorie spécifiée n'existe pas ou n'est pas active.");
            }
        }

        var entity = new Service
        {
            Numero = dto.Numero,
            Description = dto.Description,
            Prix = dto.Prix,
            ServiceCategorieID = dto.ServiceCategorieID,
            AjoutePar = dto.AjoutePar,
            DateCreation = DateTime.UtcNow
        };

        _context.Services.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with category for response
        var created = await _context.Services
            .Include(s => s.ServiceCategorie)
            .FirstOrDefaultAsync(s => s.ID == entity.ID);

        var resultDto = _mapper.Map<ServiceDto>(created);
        return CreatedAtAction(nameof(GetService), new { id = entity.ID }, resultDto);
    }

    // DELETE: api/Services/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        // Check if service is in use
        var hasTransactions = await _context.CreditTransactions.AnyAsync(ct => ct.ServiceID == id);
        var hasFactures = await _context.ElementsFactures.AnyAsync(ef => ef.ServiceID == id);
        var hasBL = await _context.BonLivraisonDetails.AnyAsync(bl => bl.ServiceID == id);

        if (hasTransactions || hasFactures || hasBL)
        {
            return BadRequest("Ce service est utilisé dans des transactions et ne peut pas être supprimé.");
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ServiceExists(int id)
    {
        return _context.Services.Any(e => e.ID == id);
    }
}
