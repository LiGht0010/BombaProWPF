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
    public class FournisseursController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public FournisseursController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Fournisseurs
        [HttpGet]
        public async Task<ActionResult<List<FournisseurDto>>> GetFournisseurs()
        {
            var fournisseurs = await _context.Fournisseurs
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<FournisseurDto>>(fournisseurs);
            return Ok(dtos);
        }

        // GET: api/Fournisseurs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FournisseurDto>> GetFournisseur(int id)
        {
            var fournisseur = await _context.Fournisseurs
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.ID == id);

            if (fournisseur == null)
                return NotFound();

            var dto = _mapper.Map<FournisseurDto>(fournisseur);
            return Ok(dto);
        }

        // GET: api/Fournisseurs/status/Actif
        [HttpGet("status/{statut}")]
        public async Task<ActionResult<List<FournisseurDto>>> GetFournisseursByStatus(string statut)
        {
            var fournisseurs = await _context.Fournisseurs
                .Where(f => f.Statut.ToLower() == statut.ToLower())
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<FournisseurDto>>(fournisseurs);
            return Ok(dtos);
        }

        // POST: api/Fournisseurs
        [HttpPost]
        public async Task<ActionResult<FournisseurDto>> CreateFournisseur([FromBody] FournisseurDto dto)
        {
            var entity = _mapper.Map<Fournisseur>(dto);
            entity.DateCreation = DateTime.UtcNow;

            // Set default status if not provided
            if (string.IsNullOrWhiteSpace(entity.Statut))
            {
                entity.Statut = "Actif";
            }

            _context.Fournisseurs.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<FournisseurDto>(entity);
            return CreatedAtAction(nameof(GetFournisseur), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Fournisseurs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFournisseur(int id, [FromBody] FournisseurDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Fournisseurs.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Fournisseurs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFournisseur(int id)
        {
            var fournisseur = await _context.Fournisseurs.FindAsync(id);
            if (fournisseur == null)
                return NotFound();

            _context.Fournisseurs.Remove(fournisseur);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
