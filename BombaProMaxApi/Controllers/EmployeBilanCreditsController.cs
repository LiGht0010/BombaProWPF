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
    public class EmployeBilanCreditsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeBilanCreditsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeBilanCredits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeBilanCredit>>> GetEmployeBilanCredits()
        {
            return await _context.EmployeBilanCredits.ToListAsync();
        }

        // GET: api/EmployeBilanCredits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeBilanCredit>> GetEmployeBilanCredit(int id)
        {
            var employeBilanCredit = await _context.EmployeBilanCredits.FindAsync(id);

            if (employeBilanCredit == null)
            {
                return NotFound();
            }

            return employeBilanCredit;
        }

        // PUT: api/EmployeBilanCredits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeBilanCredit(int id, EmployeBilanCredit employeBilanCredit)
        {
            if (id != employeBilanCredit.BilanID)
            {
                return BadRequest();
            }

            _context.Entry(employeBilanCredit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeBilanCreditExists(id))
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

        // POST: api/EmployeBilanCredits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeBilanCredit>> PostEmployeBilanCredit(EmployeBilanCredit employeBilanCredit)
        {
            _context.EmployeBilanCredits.Add(employeBilanCredit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployeBilanCredit", new { id = employeBilanCredit.BilanID }, employeBilanCredit);
        }

        // DELETE: api/EmployeBilanCredits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeBilanCredit(int id)
        {
            var employeBilanCredit = await _context.EmployeBilanCredits.FindAsync(id);
            if (employeBilanCredit == null)
            {
                return NotFound();
            }

            _context.EmployeBilanCredits.Remove(employeBilanCredit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeBilanCreditExists(int id)
        {
            return _context.EmployeBilanCredits.Any(e => e.BilanID == id);
        }
    }
}
