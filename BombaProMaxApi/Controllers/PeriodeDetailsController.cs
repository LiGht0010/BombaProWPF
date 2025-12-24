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
    public class PeriodeDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PeriodeDetailsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/PeriodeDetails
        [HttpGet]
        public async Task<ActionResult<List<PeriodeDetailsDto>>> GetPeriodeDetails()
        {
            var details = await _context.PeriodeDetails
                .Include(d => d.Pompe)
                .Include(d => d.Reservoir)
                .Include(d => d.Produit)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDetailsDto>>(details);
            return Ok(dtos);
        }

        // GET: api/PeriodeDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PeriodeDetailsDto>> GetPeriodeDetails(int id)
        {
            var detail = await _context.PeriodeDetails
                .Include(d => d.Pompe)
                .Include(d => d.Reservoir)
                .Include(d => d.Produit)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.PeriodeDetailID == id);

            if (detail == null)
                return NotFound();

            var dto = _mapper.Map<PeriodeDetailsDto>(detail);
            return Ok(dto);
        }

        // GET: api/PeriodeDetails/periode/5
        [HttpGet("periode/{periodeId}")]
        public async Task<ActionResult<List<PeriodeDetailsDto>>> GetDetailsByPeriode(int periodeId)
        {
            var details = await _context.PeriodeDetails
                .Include(d => d.Pompe)
                .Include(d => d.Reservoir)
                .Include(d => d.Produit)
                .Where(d => d.PeriodeID == periodeId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDetailsDto>>(details);
            return Ok(dtos);
        }

        // GET: api/PeriodeDetails/pompe/5
        [HttpGet("pompe/{pompeId}")]
        public async Task<ActionResult<List<PeriodeDetailsDto>>> GetDetailsByPompe(int pompeId)
        {
            var details = await _context.PeriodeDetails
                .Include(d => d.Pompe)
                .Include(d => d.Reservoir)
                .Include(d => d.Produit)
                .Where(d => d.PompeID == pompeId)
                .AsNoTracking()
                .OrderByDescending(d => d.PeriodeID)
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDetailsDto>>(details);
            return Ok(dtos);
        }

        // GET: api/PeriodeDetails/reservoir/5
        [HttpGet("reservoir/{reservoirId}")]
        public async Task<ActionResult<List<PeriodeDetailsDto>>> GetDetailsByReservoir(int reservoirId)
        {
            var details = await _context.PeriodeDetails
                .Include(d => d.Pompe)
                .Include(d => d.Reservoir)
                .Include(d => d.Produit)
                .Where(d => d.ReservoirID == reservoirId)
                .AsNoTracking()
                .OrderByDescending(d => d.PeriodeID)
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDetailsDto>>(details);
            return Ok(dtos);
        }

        // PUT: api/PeriodeDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPeriodeDetails(int id, [FromBody] PeriodeDetailsDto dto)
        {
            if (id != dto.PeriodeDetailID)
                return BadRequest("ID mismatch.");

            var existing = await _context.PeriodeDetails.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Update only the editable properties (not the computed ones)
            existing.PeriodeID = dto.PeriodeID;
            existing.PompeID = dto.PompeID;
            existing.ReservoirID = dto.ReservoirID;
            existing.ProduitID = dto.ProduitID;
            existing.PrixCarburant = dto.PrixCarburant;
            existing.CompteurElectroniqueDebut = dto.CompteurElectroniqueDebut;
            existing.CompteurElectroniqueFinal = dto.CompteurElectroniqueFinal;
            existing.CompteurMecaniqueDebut = dto.CompteurMecaniqueDebut;
            existing.CompteurMecaniqueFinal = dto.CompteurMecaniqueFinal;

            // Computed properties (QuantiteElectronique, QuantiteMecanique, etc.) 
            // are automatically calculated by the entity's expression-bodied properties

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PeriodeDetailsExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/PeriodeDetails
        [HttpPost]
        public async Task<ActionResult<PeriodeDetailsDto>> PostPeriodeDetails([FromBody] PeriodeDetailsDto dto)
        {
            try
            {
                var entity = new PeriodeDetails
                {
                    PeriodeID = dto.PeriodeID,
                    PompeID = dto.PompeID,
                    ReservoirID = dto.ReservoirID,
                    ProduitID = dto.ProduitID,
                    PrixCarburant = dto.PrixCarburant,
                    CompteurElectroniqueDebut = dto.CompteurElectroniqueDebut,
                    CompteurElectroniqueFinal = dto.CompteurElectroniqueFinal,
                    CompteurMecaniqueDebut = dto.CompteurMecaniqueDebut,
                    CompteurMecaniqueFinal = dto.CompteurMecaniqueFinal
                };

                // Computed properties are automatically calculated

                _context.PeriodeDetails.Add(entity);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var createdEntity = await _context.PeriodeDetails
                    .Include(d => d.Pompe)
                    .Include(d => d.Reservoir)
                    .Include(d => d.Produit)
                    .FirstOrDefaultAsync(d => d.PeriodeDetailID == entity.PeriodeDetailID);

                var resultDto = _mapper.Map<PeriodeDetailsDto>(createdEntity);
                return CreatedAtAction(nameof(GetPeriodeDetails), new { id = entity.PeriodeDetailID }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        // POST: api/PeriodeDetails/batch
        [HttpPost("batch")]
        public async Task<ActionResult<List<PeriodeDetailsDto>>> PostPeriodeDetailsBatch([FromBody] List<PeriodeDetailsDto> dtos)
        {
            try
            {
                var entities = new List<PeriodeDetails>();

                foreach (var dto in dtos)
                {
                    var entity = new PeriodeDetails
                    {
                        PeriodeID = dto.PeriodeID,
                        PompeID = dto.PompeID,
                        ReservoirID = dto.ReservoirID,
                        ProduitID = dto.ProduitID,
                        PrixCarburant = dto.PrixCarburant,
                        CompteurElectroniqueDebut = dto.CompteurElectroniqueDebut,
                        CompteurElectroniqueFinal = dto.CompteurElectroniqueFinal,
                        CompteurMecaniqueDebut = dto.CompteurMecaniqueDebut,
                        CompteurMecaniqueFinal = dto.CompteurMecaniqueFinal
                    };

                    entities.Add(entity);
                }

                _context.PeriodeDetails.AddRange(entities);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var ids = entities.Select(e => e.PeriodeDetailID).ToList();
                var createdEntities = await _context.PeriodeDetails
                    .Include(d => d.Pompe)
                    .Include(d => d.Reservoir)
                    .Include(d => d.Produit)
                    .Where(d => ids.Contains(d.PeriodeDetailID))
                    .ToListAsync();

                var resultDtos = _mapper.Map<List<PeriodeDetailsDto>>(createdEntities);
                return Ok(resultDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        // DELETE: api/PeriodeDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeriodeDetails(int id)
        {
            var detail = await _context.PeriodeDetails.FindAsync(id);
            if (detail == null)
                return NotFound();

            _context.PeriodeDetails.Remove(detail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/PeriodeDetails/periode/5
        [HttpDelete("periode/{periodeId}")]
        public async Task<IActionResult> DeleteDetailsByPeriode(int periodeId)
        {
            var details = await _context.PeriodeDetails
                .Where(d => d.PeriodeID == periodeId)
                .ToListAsync();

            if (!details.Any())
                return NotFound();

            _context.PeriodeDetails.RemoveRange(details);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PeriodeDetailsExists(int id)
        {
            return _context.PeriodeDetails.Any(e => e.PeriodeDetailID == id);
        }
    }
}
