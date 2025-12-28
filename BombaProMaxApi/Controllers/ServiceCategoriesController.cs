using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ServiceCategoriesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/ServiceCategories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceCategorieDto>>> GetServiceCategories()
    {
        var categories = await _context.ServiceCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Nom)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ServiceCategorieDto>>(categories));
    }

    // GET: api/ServiceCategories/all (including inactive)
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<ServiceCategorieDto>>> GetAllServiceCategories()
    {
        var categories = await _context.ServiceCategories
            .OrderBy(c => c.Nom)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ServiceCategorieDto>>(categories));
    }

    // GET: api/ServiceCategories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceCategorieDto>> GetServiceCategorie(int id)
    {
        var categorie = await _context.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ID == id);

        if (categorie == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<ServiceCategorieDto>(categorie));
    }

    // GET: api/ServiceCategories/names
    [HttpGet("names")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategorieNames()
    {
        var names = await _context.ServiceCategories
            .Where(c => c.IsActive)
            .Select(c => c.Nom)
            .OrderBy(n => n)
            .ToListAsync();

        return Ok(names);
    }

    // PUT: api/ServiceCategories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutServiceCategorie(int id, ServiceCategorieDto dto)
    {
        if (id != dto.ID)
        {
            return BadRequest("ID mismatch.");
        }

        var existing = await _context.ServiceCategories.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Check for duplicate name
        var duplicate = await _context.ServiceCategories
            .AnyAsync(c => c.ID != id && c.Nom.ToLower() == dto.Nom.ToLower());
        if (duplicate)
        {
            return BadRequest("Une catégorie avec ce nom existe déjà.");
        }

        existing.Nom = dto.Nom;
        existing.Description = dto.Description;
        existing.IsActive = dto.IsActive;
        existing.ModifiePar = dto.ModifiePar;
        existing.DateModification = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceCategorieExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // POST: api/ServiceCategories
    [HttpPost]
    public async Task<ActionResult<ServiceCategorieDto>> PostServiceCategorie(ServiceCategorieDto dto)
    {
        // Check for duplicate name
        var duplicate = await _context.ServiceCategories
            .AnyAsync(c => c.Nom.ToLower() == dto.Nom.ToLower());
        if (duplicate)
        {
            return BadRequest("Une catégorie avec ce nom existe déjà.");
        }

        var entity = new ServiceCategorie
        {
            Nom = dto.Nom,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreePar = dto.CreePar,
            DateCreation = DateTime.UtcNow
        };

        _context.ServiceCategories.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<ServiceCategorieDto>(entity);
        return CreatedAtAction(nameof(GetServiceCategorie), new { id = entity.ID }, resultDto);
    }

    // DELETE: api/ServiceCategories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceCategorie(int id)
    {
        var categorie = await _context.ServiceCategories.FindAsync(id);
        if (categorie == null)
        {
            return NotFound();
        }

        // Soft delete - just deactivate
        categorie.IsActive = false;
        categorie.DateModification = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/ServiceCategories/5/permanent
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> DeleteServiceCategoriePermanent(int id)
    {
        var categorie = await _context.ServiceCategories.FindAsync(id);
        if (categorie == null)
        {
            return NotFound();
        }

        // Check if category is in use
        var hasServices = await _context.Services.AnyAsync(s => s.ServiceCategorieID == id);
        if (hasServices)
        {
            return BadRequest("Cette catégorie est utilisée par des services et ne peut pas être supprimée.");
        }

        _context.ServiceCategories.Remove(categorie);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ServiceCategorieExists(int id)
    {
        return _context.ServiceCategories.Any(e => e.ID == id);
    }
}
