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
    public class ReglementCreditsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReglementCreditsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/ReglementCredits
        [HttpGet]
        public async Task<ActionResult<List<ReglementCreditDto>>> GetReglementsCredit()
        {
            var reglements = await _context.ReglementsCredit
                .Include(r => r.Client)
                .Include(r => r.ModePaiement)
                .AsNoTracking()
                .OrderByDescending(r => r.DateReglement)
                .ToListAsync();

            var dtos = _mapper.Map<List<ReglementCreditDto>>(reglements);
            return Ok(dtos);
        }

        // GET: api/ReglementCredits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReglementCreditDto>> GetReglementCredit(int id)
        {
            var reglement = await _context.ReglementsCredit
                .Include(r => r.Client)
                .Include(r => r.ModePaiement)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReglementID == id);

            if (reglement == null)
                return NotFound();

            var dto = _mapper.Map<ReglementCreditDto>(reglement);
            return Ok(dto);
        }

        // GET: api/ReglementCredits/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<List<ReglementCreditDto>>> GetReglementsByClient(int clientId)
        {
            var reglements = await _context.ReglementsCredit
                .Include(r => r.Client)
                .Include(r => r.ModePaiement)
                .Where(r => r.ClientID == clientId)
                .AsNoTracking()
                .OrderByDescending(r => r.DateReglement)
                .ToListAsync();

            var dtos = _mapper.Map<List<ReglementCreditDto>>(reglements);
            return Ok(dtos);
        }

        // GET: api/ReglementCredits/client/5/total
        [HttpGet("client/{clientId}/total")]
        public async Task<ActionResult<decimal>> GetTotalByClient(int clientId)
        {
            var total = await _context.ReglementsCredit
                .Where(r => r.ClientID == clientId)
                .SumAsync(r => r.MontantPaye);

            return Ok(total);
        }

        // PUT: api/ReglementCredits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReglementCredit(int id, [FromBody] ReglementCreditDto dto)
        {
            if (id != dto.ReglementID)
                return BadRequest("ID mismatch.");

            var existing = await _context.ReglementsCredit.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            
            // Ensure all DateTime values are in UTC for PostgreSQL
            existing.DateModification = DateTime.UtcNow;
            existing.DateReglement = DateTime.SpecifyKind(existing.DateReglement, DateTimeKind.Utc);
            
            if (existing.DateCreation.HasValue)
            {
                existing.DateCreation = DateTime.SpecifyKind(existing.DateCreation.Value, DateTimeKind.Utc);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReglementCreditExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/ReglementCredits
        [HttpPost]
        public async Task<ActionResult<ReglementCreditDto>> PostReglementCredit([FromBody] ReglementCreditDto dto)
        {
            try
            {
                var entity = _mapper.Map<ReglementCredit>(dto);
                
                // Ensure all DateTime values are in UTC for PostgreSQL
                entity.DateCreation = DateTime.UtcNow;
                entity.DateReglement = DateTime.SpecifyKind(entity.DateReglement, DateTimeKind.Utc);
                
                if (entity.DateModification.HasValue)
                {
                    entity.DateModification = DateTime.SpecifyKind(entity.DateModification.Value, DateTimeKind.Utc);
                }

                _context.ReglementsCredit.Add(entity);
                await _context.SaveChangesAsync();

                // Reload with navigation properties to get display names
                var createdEntity = await _context.ReglementsCredit
                    .Include(r => r.Client)
                    .Include(r => r.ModePaiement)
                    .FirstOrDefaultAsync(r => r.ReglementID == entity.ReglementID);

                var resultDto = _mapper.Map<ReglementCreditDto>(createdEntity);
                return CreatedAtAction(nameof(GetReglementCredit), new { id = entity.ReglementID }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        // DELETE: api/ReglementCredits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReglementCredit(int id)
        {
            var reglement = await _context.ReglementsCredit.FindAsync(id);
            if (reglement == null)
                return NotFound();

            _context.ReglementsCredit.Remove(reglement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReglementCreditExists(int id)
        {
            return _context.ReglementsCredit.Any(e => e.ReglementID == id);
        }
    }
}
