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
    public class ReservoirsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReservoirsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Reservoirs
        [HttpGet]
        public async Task<ActionResult<List<ReservoirDto>>> GetReservoirs()
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.Calibrations)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirDto>>(reservoirs);
            return Ok(dtos);
        }

        // GET: api/Reservoirs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservoirDto>> GetReservoir(int id)
        {
            var reservoir = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.Calibrations)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservoir == null)
                return NotFound();

            var dto = _mapper.Map<ReservoirDto>(reservoir);
            return Ok(dto);
        }

        // GET: api/Reservoirs/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<ReservoirDto>>> GetReservoirsByProduct(int productId)
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.Calibrations)
                .Where(r => r.ProduitID == productId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirDto>>(reservoirs);
            return Ok(dtos);
        }

        // GET: api/Reservoirs/lowfuel?threshold=20
        [HttpGet("lowfuel")]
        public async Task<ActionResult<List<ReservoirDto>>> GetLowFuelReservoirs([FromQuery] decimal threshold = 20)
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Where(r => r.Capacite > 0 && (r.NiveauDeCarburant / r.Capacite * 100) < threshold)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirDto>>(reservoirs);
            return Ok(dtos);
        }

        // GET: api/Reservoirs/check-numero?numero=xxx&excludeId=1
        [HttpGet("check-numero")]
        public async Task<ActionResult<bool>> CheckNumeroExists([FromQuery] string numero, [FromQuery] int? excludeId = null)
        {
            var exists = await _context.Reservoirs
                .AnyAsync(r => r.Numero.ToLower() == numero.ToLower() && r.ID != excludeId);
            return Ok(exists);
        }

        // POST: api/Reservoirs
        [HttpPost]
        public async Task<ActionResult<ReservoirDto>> CreateReservoir([FromBody] ReservoirDto dto)
        {
            var entity = _mapper.Map<Reservoir>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Reservoirs.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with Produit for response
            if (entity.ProduitID.HasValue)
            {
                await _context.Entry(entity).Reference(r => r.Produit).LoadAsync();
            }

            var resultDto = _mapper.Map<ReservoirDto>(entity);
            return CreatedAtAction(nameof(GetReservoir), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Reservoirs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservoir(int id, [FromBody] ReservoirDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Reservoirs.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Reservoirs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservoir(int id)
        {
            var reservoir = await _context.Reservoirs.FindAsync(id);
            if (reservoir == null)
                return NotFound();

            _context.Reservoirs.Remove(reservoir);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
