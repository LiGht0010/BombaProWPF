using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepensesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DepensesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Depenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepenseDto>>> GetDepenses()
        {
            var depenses = await _context.Depenses
                .OrderByDescending(d => d.Date)
                .ThenByDescending(d => d.ID)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<List<DepenseDto>>(depenses));
        }

        // GET: api/Depenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepenseDto>> GetDepense(int id)
        {
            var depense = await _context.Depenses
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.ID == id);

            if (depense == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<DepenseDto>(depense));
        }

        // GET: api/Depenses/bydate?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("bydate")]
        public async Task<ActionResult<IEnumerable<DepenseDto>>> GetDepensesByDateRange(
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            var depenses = await _context.Depenses
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .OrderByDescending(d => d.Date)
                .ThenByDescending(d => d.ID)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<List<DepenseDto>>(depenses));
        }

        // GET: api/Depenses/bycategory/{categorie}
        [HttpGet("bycategory/{categorie}")]
        public async Task<ActionResult<IEnumerable<DepenseDto>>> GetDepensesByCategory(string categorie)
        {
            var depenses = await _context.Depenses
                .Where(d => d.Categorie != null && d.Categorie.ToLower() == categorie.ToLower())
                .OrderByDescending(d => d.Date)
                .ThenByDescending(d => d.ID)
                .AsNoTracking()
                .ToListAsync();

            return Ok(_mapper.Map<List<DepenseDto>>(depenses));
        }

        // GET: api/Depenses/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var categories = await _context.Depenses
                .Where(d => d.Categorie != null)
                .Select(d => d.Categorie!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Depenses/summary?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetDepensesSummary(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var query = _context.Depenses.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(d => d.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(d => d.Date <= endDate.Value);

            var depenses = await query.ToListAsync();

            var summary = new
            {
                TotalDepenses = depenses.Count,
                TotalMontant = depenses.Sum(d => d.Montant ?? 0),
                ParCategorie = depenses
                    .Where(d => d.Categorie != null)
                    .GroupBy(d => d.Categorie)
                    .Select(g => new { Categorie = g.Key, Total = g.Sum(d => d.Montant ?? 0) })
                    .OrderByDescending(x => x.Total)
                    .ToList()
            };

            return Ok(summary);
        }

        // PUT: api/Depenses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepense(int id, DepenseDto dto)
        {
            if (id != dto.ID)
            {
                return BadRequest("ID mismatch.");
            }

            var existing = await _context.Depenses.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Update properties
            existing.Numero = dto.Numero;
            existing.Date = dto.Date;
            existing.Categorie = dto.Categorie;
            existing.Montant = dto.Montant;
            existing.Description = dto.Description;
            existing.ModifiePar = dto.ModifiePar;
            existing.DateModification = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepenseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Depenses
        [HttpPost]
        public async Task<ActionResult<DepenseDto>> PostDepense(DepenseDto dto)
        {
            try
            {
                var entity = new Depense
                {
                    Numero = dto.Numero,
                    Date = dto.Date ?? DateOnly.FromDateTime(DateTime.Today),
                    Categorie = dto.Categorie,
                    Montant = dto.Montant,
                    Description = dto.Description,
                    CreePar = dto.CreePar,
                    DateCreation = DateTime.UtcNow
                };

                // Generate numero if not provided
                if (string.IsNullOrEmpty(entity.Numero))
                {
                    entity.Numero = GenerateNumero();
                }

                _context.Depenses.Add(entity);
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<DepenseDto>(entity);
                return CreatedAtAction(nameof(GetDepense), new { id = entity.ID }, resultDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating depense: {ex.Message}");
            }
        }

        // DELETE: api/Depenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepense(int id)
        {
            var depense = await _context.Depenses.FindAsync(id);
            if (depense == null)
            {
                return NotFound();
            }

            _context.Depenses.Remove(depense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepenseExists(int id)
        {
            return _context.Depenses.Any(e => e.ID == id);
        }

        private string GenerateNumero()
        {
            var date = DateTime.Now;
            return $"DEP-{date:yyyyMMdd}-{date:HHmmss}";
        }
    }
}
