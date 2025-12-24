using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndicateursFinanciersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IndicateursFinanciersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/IndicateursFinanciers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IndicateursFinancier>>> GetIndicateursFinanciers()
        {
            return await _context.IndicateursFinanciers.ToListAsync();
        }

        // GET: api/IndicateursFinanciers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IndicateursFinancier>> GetIndicateursFinancier(int id)
        {
            var indicateursFinancier = await _context.IndicateursFinanciers.FindAsync(id);

            if (indicateursFinancier == null)
            {
                return NotFound();
            }

            return indicateursFinancier;
        }

        // PUT: api/IndicateursFinanciers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIndicateursFinancier(int id, IndicateursFinancier indicateursFinancier)
        {
            if (id != indicateursFinancier.ID)
            {
                return BadRequest();
            }

            _context.Entry(indicateursFinancier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IndicateursFinancierExists(id))
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

        // POST: api/IndicateursFinanciers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IndicateursFinancier>> PostIndicateursFinancier(IndicateursFinancier indicateursFinancier)
        {
            _context.IndicateursFinanciers.Add(indicateursFinancier);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIndicateursFinancier", new { id = indicateursFinancier.ID }, indicateursFinancier);
        }

        // DELETE: api/IndicateursFinanciers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIndicateursFinancier(int id)
        {
            var indicateursFinancier = await _context.IndicateursFinanciers.FindAsync(id);
            if (indicateursFinancier == null)
            {
                return NotFound();
            }

            _context.IndicateursFinanciers.Remove(indicateursFinancier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IndicateursFinancierExists(int id)
        {
            return _context.IndicateursFinanciers.Any(e => e.ID == id);
        }
    }
}
