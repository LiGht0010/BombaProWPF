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
    public class MoyensPaiementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MoyensPaiementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MoyensPaiements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MoyensPaiement>>> GetMoyensPaiements()
        {
            return await _context.MoyensPaiements.ToListAsync();
        }

        // GET: api/MoyensPaiements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MoyensPaiement>> GetMoyensPaiement(int id)
        {
            var moyensPaiement = await _context.MoyensPaiements.FindAsync(id);

            if (moyensPaiement == null)
            {
                return NotFound();
            }

            return moyensPaiement;
        }

        // PUT: api/MoyensPaiements/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMoyensPaiement(int id, MoyensPaiement moyensPaiement)
        {
            if (id != moyensPaiement.ID)
            {
                return BadRequest();
            }

            _context.Entry(moyensPaiement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MoyensPaiementExists(id))
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

        // POST: api/MoyensPaiements
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MoyensPaiement>> PostMoyensPaiement(MoyensPaiement moyensPaiement)
        {
            _context.MoyensPaiements.Add(moyensPaiement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMoyensPaiement", new { id = moyensPaiement.ID }, moyensPaiement);
        }

        // DELETE: api/MoyensPaiements/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMoyensPaiement(int id)
        {
            var moyensPaiement = await _context.MoyensPaiements.FindAsync(id);
            if (moyensPaiement == null)
            {
                return NotFound();
            }

            _context.MoyensPaiements.Remove(moyensPaiement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MoyensPaiementExists(int id)
        {
            return _context.MoyensPaiements.Any(e => e.ID == id);
        }
    }
}
