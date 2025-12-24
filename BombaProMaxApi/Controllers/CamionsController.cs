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
    public class CamionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CamionsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Camions
        [HttpGet]
        public async Task<ActionResult<List<CamionDto>>> GetCamions()
        {
            var camions = await _context.Camions
                .Include(c => c.Fournisseur)
                .Include(c => c.Citerne)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<CamionDto>>(camions);
            return Ok(dtos);
        }

        // GET: api/Camions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CamionDto>> GetCamion(int id)
        {
            var camion = await _context.Camions
                .Include(c => c.Fournisseur)
                .Include(c => c.Citerne)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == id);

            if (camion == null)
                return NotFound();

            var dto = _mapper.Map<CamionDto>(camion);
            return Ok(dto);
        }

        // GET: api/Camions/fournisseur/5
        [HttpGet("fournisseur/{fournisseurId}")]
        public async Task<ActionResult<List<CamionDto>>> GetCamionsByFournisseur(int fournisseurId)
        {
            var camions = await _context.Camions
                .Include(c => c.Fournisseur)
                .Include(c => c.Citerne)
                .Where(c => c.FournisseurID == fournisseurId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<CamionDto>>(camions);
            return Ok(dtos);
        }

        // GET: api/Camions/available
        [HttpGet("available")]
        public async Task<ActionResult<List<CamionDto>>> GetAvailableCamions()
        {
            var camions = await _context.Camions
                .Include(c => c.Fournisseur)
                .Include(c => c.Citerne)
                .Where(c => c.CiterneID == null)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<CamionDto>>(camions);
            return Ok(dtos);
        }

        // POST: api/Camions
        [HttpPost]
        public async Task<ActionResult<CamionDto>> CreateCamion([FromBody] CamionDto dto)
        {
            var entity = _mapper.Map<Camion>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Camions.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with navigation properties for response
            await _context.Entry(entity).Reference(c => c.Fournisseur).LoadAsync();
            if (entity.CiterneID.HasValue)
            {
                await _context.Entry(entity).Reference(c => c.Citerne).LoadAsync();
            }

            var resultDto = _mapper.Map<CamionDto>(entity);
            return CreatedAtAction(nameof(GetCamion), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Camions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCamion(int id, [FromBody] CamionDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Camions.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Camions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamion(int id)
        {
            var camion = await _context.Camions.FindAsync(id);
            if (camion == null)
                return NotFound();

            _context.Camions.Remove(camion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
