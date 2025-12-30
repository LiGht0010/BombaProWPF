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
    public class BilanCreditsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BilanCreditsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/BilanCredits
        [HttpGet]
        public async Task<ActionResult<List<BilanCreditDto>>> GetBilansCredit()
        {
            var bilans = await _context.BilansCredit
                .Include(b => b.Client)
                .AsNoTracking()
                .OrderByDescending(b => b.DerniereMiseAJour)
                .ToListAsync();

            var dtos = _mapper.Map<List<BilanCreditDto>>(bilans);
            return Ok(dtos);
        }

        // GET: api/BilanCredits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BilanCreditDto>> GetBilanCredit(int id)
        {
            var bilan = await _context.BilansCredit
                .Include(b => b.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BilanID == id);

            if (bilan == null)
                return NotFound();

            var dto = _mapper.Map<BilanCreditDto>(bilan);
            return Ok(dto);
        }

        // GET: api/BilanCredits/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<BilanCreditDto>> GetBilanByClient(int clientId)
        {
            var bilan = await _context.BilansCredit
                .Include(b => b.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ClientID == clientId);

            if (bilan == null)
            {
                // Return a default empty bilan if none exists
                return Ok(new BilanCreditDto
                {
                    ClientID = clientId,
                    TotalCredit = 0,
                    TotalPaye = 0,
                    Balance = 0,
                    CreditFacture = 0,
                    CreditNonFacture = 0,
                    DerniereMiseAJour = DateTime.UtcNow
                });
            }

            var dto = _mapper.Map<BilanCreditDto>(bilan);
            return Ok(dto);
        }

        // PUT: api/BilanCredits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBilanCredit(int id, [FromBody] BilanCreditDto dto)
        {
            if (id != dto.BilanID)
                return BadRequest("ID mismatch.");

            var existing = await _context.BilansCredit.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DerniereMiseAJour = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BilanCreditExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/BilanCredits
        [HttpPost]
        public async Task<ActionResult<BilanCreditDto>> PostBilanCredit([FromBody] BilanCreditDto dto)
        {
            var entity = _mapper.Map<BilanCredit>(dto);
            entity.DerniereMiseAJour = DateTime.UtcNow;

            _context.BilansCredit.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<BilanCreditDto>(entity);
            return CreatedAtAction(nameof(GetBilanCredit), new { id = entity.BilanID }, resultDto);
        }

        // POST: api/BilanCredits/recalculate/5
        [HttpPost("recalculate/{clientId}")]
        public async Task<ActionResult<BilanCreditDto>> RecalculateBilan(int clientId)
        {
            // Calculate totals from transactions and reglements
            var totalCredit = await _context.CreditTransactions
                .Where(t => t.ClientID == clientId)
                .SumAsync(t => t.MontantTotal);

            var creditFacture = await _context.CreditTransactions
                .Where(t => t.ClientID == clientId && t.Facture)
                .SumAsync(t => t.MontantTotal);

            var creditNonFacture = await _context.CreditTransactions
                .Where(t => t.ClientID == clientId && !t.Facture)
                .SumAsync(t => t.MontantTotal);

            var totalPaye = await _context.ReglementsCredit
                .Where(r => r.ClientID == clientId)
                .SumAsync(r => r.MontantPaye);

            // Balance = TotalPaye - TotalCredit
            // Positive balance = client overpaid (green)
            // Negative balance = client owes money (red)
            var balance = totalPaye - totalCredit;

            // Find or create bilan
            var bilan = await _context.BilansCredit
                .FirstOrDefaultAsync(b => b.ClientID == clientId);

            if (bilan == null)
            {
                bilan = new BilanCredit
                {
                    ClientID = clientId,
                    TotalCredit = totalCredit,
                    TotalPaye = totalPaye,
                    Balance = balance,
                    CreditFacture = creditFacture,
                    CreditNonFacture = creditNonFacture,
                    DerniereMiseAJour = DateTime.UtcNow
                };
                _context.BilansCredit.Add(bilan);
            }
            else
            {
                bilan.TotalCredit = totalCredit;
                bilan.TotalPaye = totalPaye;
                bilan.Balance = balance;
                bilan.CreditFacture = creditFacture;
                bilan.CreditNonFacture = creditNonFacture;
                bilan.DerniereMiseAJour = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Reload with navigation properties
            bilan = await _context.BilansCredit
                .Include(b => b.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BilanID == bilan.BilanID);

            var dto = _mapper.Map<BilanCreditDto>(bilan);
            return Ok(dto);
        }

        // DELETE: api/BilanCredits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilanCredit(int id)
        {
            var bilan = await _context.BilansCredit.FindAsync(id);
            if (bilan == null)
                return NotFound();

            _context.BilansCredit.Remove(bilan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BilanCreditExists(int id)
        {
            return _context.BilansCredit.Any(e => e.BilanID == id);
        }
    }
}
