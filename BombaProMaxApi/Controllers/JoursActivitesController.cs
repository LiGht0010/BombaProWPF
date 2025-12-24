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
    public class JoursActivitesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JoursActivitesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/JoursActivites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JoursActivite>>> GetJoursActivites()
        {
            return await _context.JoursActivites.ToListAsync();
        }

        // GET: api/JoursActivites/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JoursActivite>> GetJoursActivite(int id)
        {
            var joursActivite = await _context.JoursActivites.FindAsync(id);

            if (joursActivite == null)
            {
                return NotFound();
            }

            return joursActivite;
        }

        // PUT: api/JoursActivites/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJoursActivite(int id, JoursActivite joursActivite)
        {
            if (id != joursActivite.ID)
            {
                return BadRequest();
            }

            _context.Entry(joursActivite).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JoursActiviteExists(id))
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

        // POST: api/JoursActivites
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<JoursActivite>> PostJoursActivite(JoursActivite joursActivite)
        {
            _context.JoursActivites.Add(joursActivite);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJoursActivite", new { id = joursActivite.ID }, joursActivite);
        }

        // DELETE: api/JoursActivites/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJoursActivite(int id)
        {
            var joursActivite = await _context.JoursActivites.FindAsync(id);
            if (joursActivite == null)
            {
                return NotFound();
            }

            _context.JoursActivites.Remove(joursActivite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JoursActiviteExists(int id)
        {
            return _context.JoursActivites.Any(e => e.ID == id);
        }
    }
}
