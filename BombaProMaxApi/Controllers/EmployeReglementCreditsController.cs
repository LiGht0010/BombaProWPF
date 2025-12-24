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
    public class EmployeReglementCreditsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeReglementCreditsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeReglementCredits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeReglementCredit>>> GetEmployeReglementCredits()
        {
            return await _context.EmployeReglementCredits.ToListAsync();
        }

        // GET: api/EmployeReglementCredits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeReglementCredit>> GetEmployeReglementCredit(int id)
        {
            var employeReglementCredit = await _context.EmployeReglementCredits.FindAsync(id);

            if (employeReglementCredit == null)
            {
                return NotFound();
            }

            return employeReglementCredit;
        }

        // PUT: api/EmployeReglementCredits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeReglementCredit(int id, EmployeReglementCredit employeReglementCredit)
        {
            if (id != employeReglementCredit.ReglementID)
            {
                return BadRequest();
            }

            _context.Entry(employeReglementCredit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeReglementCreditExists(id))
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

        // POST: api/EmployeReglementCredits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeReglementCredit>> PostEmployeReglementCredit(EmployeReglementCredit employeReglementCredit)
        {
            _context.EmployeReglementCredits.Add(employeReglementCredit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployeReglementCredit", new { id = employeReglementCredit.ReglementID }, employeReglementCredit);
        }

        // DELETE: api/EmployeReglementCredits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeReglementCredit(int id)
        {
            var employeReglementCredit = await _context.EmployeReglementCredits.FindAsync(id);
            if (employeReglementCredit == null)
            {
                return NotFound();
            }

            _context.EmployeReglementCredits.Remove(employeReglementCredit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeReglementCreditExists(int id)
        {
            return _context.EmployeReglementCredits.Any(e => e.ReglementID == id);
        }
    }
}
