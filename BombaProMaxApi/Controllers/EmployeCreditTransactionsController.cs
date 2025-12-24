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
    public class EmployeCreditTransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeCreditTransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeCreditTransactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeCreditTransaction>>> GetEmployeCreditTransactions()
        {
            return await _context.EmployeCreditTransactions.ToListAsync();
        }

        // GET: api/EmployeCreditTransactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeCreditTransaction>> GetEmployeCreditTransaction(int id)
        {
            var employeCreditTransaction = await _context.EmployeCreditTransactions.FindAsync(id);

            if (employeCreditTransaction == null)
            {
                return NotFound();
            }

            return employeCreditTransaction;
        }

        // PUT: api/EmployeCreditTransactions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeCreditTransaction(int id, EmployeCreditTransaction employeCreditTransaction)
        {
            if (id != employeCreditTransaction.CreditID)
            {
                return BadRequest();
            }

            _context.Entry(employeCreditTransaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeCreditTransactionExists(id))
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

        // POST: api/EmployeCreditTransactions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeCreditTransaction>> PostEmployeCreditTransaction(EmployeCreditTransaction employeCreditTransaction)
        {
            _context.EmployeCreditTransactions.Add(employeCreditTransaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmployeCreditTransaction", new { id = employeCreditTransaction.CreditID }, employeCreditTransaction);
        }

        // DELETE: api/EmployeCreditTransactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeCreditTransaction(int id)
        {
            var employeCreditTransaction = await _context.EmployeCreditTransactions.FindAsync(id);
            if (employeCreditTransaction == null)
            {
                return NotFound();
            }

            _context.EmployeCreditTransactions.Remove(employeCreditTransaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeCreditTransactionExists(int id)
        {
            return _context.EmployeCreditTransactions.Any(e => e.CreditID == id);
        }
    }
}
