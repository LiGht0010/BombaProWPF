using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepenseCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DepenseCategoriesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/DepenseCategories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepenseCategorieDto>>> GetDepenseCategories()
    {
        var categories = await _context.DepenseCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Nom)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<DepenseCategorieDto>>(categories));
    }

    // GET: api/DepenseCategories/all (including inactive)
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<DepenseCategorieDto>>> GetAllDepenseCategories()
    {
        var categories = await _context.DepenseCategories
            .OrderBy(c => c.Nom)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<DepenseCategorieDto>>(categories));
    }

    // GET: api/DepenseCategories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DepenseCategorieDto>> GetDepenseCategorie(int id)
    {
        var categorie = await _context.DepenseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ID == id);

        if (categorie == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<DepenseCategorieDto>(categorie));
    }

    // GET: api/DepenseCategories/names
    [HttpGet("names")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategorieNames()
    {
        var names = await _context.DepenseCategories
            .Where(c => c.IsActive)
            .Select(c => c.Nom)
            .OrderBy(n => n)
            .ToListAsync();

        return Ok(names);
    }

    // PUT: api/DepenseCategories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDepenseCategorie(int id, DepenseCategorieDto dto)
    {
        if (id != dto.ID)
        {
            return BadRequest("ID mismatch.");
        }

        var existing = await _context.DepenseCategories.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Check for duplicate name
        var duplicate = await _context.DepenseCategories
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
            if (!DepenseCategorieExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // POST: api/DepenseCategories
    [HttpPost]
    public async Task<ActionResult<DepenseCategorieDto>> PostDepenseCategorie(DepenseCategorieDto dto)
    {
        // Check for duplicate name
        var duplicate = await _context.DepenseCategories
            .AnyAsync(c => c.Nom.ToLower() == dto.Nom.ToLower());
        if (duplicate)
        {
            return BadRequest("Une catégorie avec ce nom existe déjà.");
        }

        var entity = new DepenseCategorie
        {
            Nom = dto.Nom,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreePar = dto.CreePar,
            DateCreation = DateTime.UtcNow
        };

        _context.DepenseCategories.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<DepenseCategorieDto>(entity);
        return CreatedAtAction(nameof(GetDepenseCategorie), new { id = entity.ID }, resultDto);
    }

    // DELETE: api/DepenseCategories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepenseCategorie(int id)
    {
        var categorie = await _context.DepenseCategories.FindAsync(id);
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

    // DELETE: api/DepenseCategories/5/permanent
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> DeleteDepenseCategoriePermanent(int id)
    {
        var categorie = await _context.DepenseCategories.FindAsync(id);
        if (categorie == null)
        {
            return NotFound();
        }

        _context.DepenseCategories.Remove(categorie);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DepenseCategorieExists(int id)
    {
        return _context.DepenseCategories.Any(e => e.ID == id);
    }
}
