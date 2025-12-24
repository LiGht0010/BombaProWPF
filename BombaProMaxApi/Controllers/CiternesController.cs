using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CiternesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CiternesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Citernes
        [HttpGet]
        public async Task<ActionResult<List<CiterneDto>>> GetCiternes()
        {
            var citernes = await _context.Citernes
                .Include(c => c.Fournisseur)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<CiterneDto>>(citernes);
            return Ok(dtos);
        }

        // GET: api/Citernes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CiterneDto>> GetCiterne(int id)
        {
            var citerne = await _context.Citernes
                .Include(c => c.Fournisseur)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == id);

            if (citerne == null)
                return NotFound();

            var dto = _mapper.Map<CiterneDto>(citerne);
            return Ok(dto);
        }

        // GET: api/Citernes/fournisseur/5
        [HttpGet("fournisseur/{fournisseurId}")]
        public async Task<ActionResult<List<CiterneDto>>> GetCiternesByFournisseur(int fournisseurId)
        {
            var citernes = await _context.Citernes
                .Include(c => c.Fournisseur)
                .Where(c => c.FournisseurID == fournisseurId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<CiterneDto>>(citernes);
            return Ok(dtos);
        }

        // POST: api/Citernes
        [HttpPost]
        public async Task<ActionResult<CiterneDto>> CreateCiterne([FromBody] CiterneDto dto)
        {
            var entity = _mapper.Map<Citerne>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Citernes.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with Fournisseur for response
            await _context.Entry(entity).Reference(c => c.Fournisseur).LoadAsync();

            var resultDto = _mapper.Map<CiterneDto>(entity);
            return CreatedAtAction(nameof(GetCiterne), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Citernes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCiterne(int id, [FromBody] CiterneDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Citernes.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Citernes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCiterne(int id)
        {
            var citerne = await _context.Citernes.FindAsync(id);
            if (citerne == null)
                return NotFound();

            _context.Citernes.Remove(citerne);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
