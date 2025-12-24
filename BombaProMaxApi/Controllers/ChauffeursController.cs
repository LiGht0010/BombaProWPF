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
    public class ChauffeursController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ChauffeursController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/chauffeurs
        [HttpGet]
        public async Task<ActionResult<List<ChauffeurDto>>> GetChauffeurs()
        {
            var chauffeurs = await _context.Chauffeurs
                .Include(c => c.Fournisseur)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ChauffeurDto>>(chauffeurs);
            return Ok(dtos);
        }

        // GET: api/chauffeurs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChauffeurDto>> GetChauffeur(int id)
        {
            var chauffeur = await _context.Chauffeurs
                .Include(c => c.Fournisseur)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == id);

            if (chauffeur == null)
                return NotFound();

            var dto = _mapper.Map<ChauffeurDto>(chauffeur);
            return Ok(dto);
        }

        // GET: api/chauffeurs/fournisseur/5
        [HttpGet("fournisseur/{fournisseurId}")]
        public async Task<ActionResult<List<ChauffeurDto>>> GetChauffeursByFournisseur(int fournisseurId)
        {
            var chauffeurs = await _context.Chauffeurs
                .Include(c => c.Fournisseur)
                .Where(c => c.FournisseurID == fournisseurId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ChauffeurDto>>(chauffeurs);
            return Ok(dtos);
        }

        // POST: api/chauffeurs
        [HttpPost]
        public async Task<ActionResult<ChauffeurDto>> CreateChauffeur([FromBody] ChauffeurDto dto)
        {
            var entity = _mapper.Map<Chauffeur>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Chauffeurs.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with fournisseur for response mapping
            await _context.Entry(entity).Reference(e => e.Fournisseur).LoadAsync();

            var resultDto = _mapper.Map<ChauffeurDto>(entity);
            return CreatedAtAction(nameof(GetChauffeur), new { id = entity.ID }, resultDto);
        }

        // PUT: api/chauffeurs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChauffeur(int id, [FromBody] ChauffeurDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Chauffeurs.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/chauffeurs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChauffeur(int id)
        {
            var chauffeur = await _context.Chauffeurs.FindAsync(id);
            if (chauffeur == null)
                return NotFound();

            _context.Chauffeurs.Remove(chauffeur);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}