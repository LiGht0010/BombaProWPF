using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PompesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PompesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/pompe
        [HttpGet]
        public async Task<ActionResult<List<PompeDto>>> GetPompes()
        {
            var pompes = await _context.Pompes
                .Include(p => p.ReservoirAssocie)
                    .ThenInclude(r => r.Produit)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<PompeDto>>(pompes);
            return Ok(dtos);
        }

        // GET: api/pompe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PompeDto>> GetPompe(int id)
        {
            var pompe = await _context.Pompes
                .Include(p => p.ReservoirAssocie)
                    .ThenInclude(r => r.Produit)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == id);

            if (pompe == null)
                return NotFound();

            var dto = _mapper.Map<PompeDto>(pompe);
            return Ok(dto);
        }

        // POST: api/pompe
        [HttpPost]
        public async Task<ActionResult<PompeDto>> CreatePompe([FromBody] PompeDto dto)
        {
            var entity = _mapper.Map<Pompe>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Pompes.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with reservoir and produit for response mapping
            await _context.Entry(entity).Reference(e => e.ReservoirAssocie).LoadAsync();
            if (entity.ReservoirAssocie != null)
            {
                await _context.Entry(entity.ReservoirAssocie).Reference(r => r.Produit).LoadAsync();
            }

            var resultDto = _mapper.Map<PompeDto>(entity);
            return CreatedAtAction(nameof(GetPompe), new { id = entity.ID }, resultDto);
        }

        // PUT: api/pompe/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePompe(int id, [FromBody] PompeDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Pompes.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/pompe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePompe(int id)
        {
            var pompe = await _context.Pompes.FindAsync(id);
            if (pompe == null)
                return NotFound();

            _context.Pompes.Remove(pompe);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
