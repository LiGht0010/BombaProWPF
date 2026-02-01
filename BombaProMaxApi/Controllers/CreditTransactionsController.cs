using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CreditTransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CreditTransactionsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/CreditTransactions
    [HttpGet]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetCreditTransactions()
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Include(t => t.FactureAssociee)
            .Include(t => t.BonLivraison)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    // GET: api/CreditTransactions/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CreditTransactionDto>> GetCreditTransaction(int id)
    {
        var transaction = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Include(t => t.FactureAssociee)
            .Include(t => t.BonLivraison)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.CreditID == id);

        if (transaction == null)
            return NotFound();

        var dto = _mapper.Map<CreditTransactionDto>(transaction);
        return Ok(dto);
    }

    // GET: api/CreditTransactions/client/5
    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetTransactionsByClient(int clientId)
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Include(t => t.FactureAssociee)
            .Include(t => t.BonLivraison)
            .Where(t => t.ClientID == clientId)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    // GET: api/CreditTransactions/client/5/non-invoiced
    [HttpGet("client/{clientId}/non-invoiced")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetNonInvoicedTransactionsByClient(int clientId)
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Where(t => t.ClientID == clientId && !t.Facture)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    // GET: api/CreditTransactions/client/5/available (not in BL and not invoiced)
    [HttpGet("client/{clientId}/available")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetAvailableTransactionsByClient(int clientId)
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Where(t => t.ClientID == clientId && !t.EstEnBL && !t.Facture)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    // GET: api/CreditTransactions/client/5/in-bl (in BL but not yet invoiced)
    [HttpGet("client/{clientId}/in-bl")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetInBLTransactionsByClient(int clientId)
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Include(t => t.BonLivraison)
            .Where(t => t.ClientID == clientId && t.EstEnBL && !t.Facture)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    // GET: api/CreditTransactions/client/5/invoiced (already invoiced)
    [HttpGet("client/{clientId}/invoiced")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetInvoicedTransactionsByClient(int clientId)
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Include(t => t.Service)
            .Include(t => t.FactureAssociee)
            .Include(t => t.BonLivraison)
            .Where(t => t.ClientID == clientId && t.Facture)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    // GET: api/CreditTransactions/client/5/total
    [HttpGet("client/{clientId}/total")]
    public async Task<ActionResult<decimal>> GetTotalByClient(int clientId)
    {
        var total = await _context.CreditTransactions
            .Where(t => t.ClientID == clientId)
            .SumAsync(t => t.MontantTotal);

        return Ok(total);
    }

    // GET: api/CreditTransactions/client/5/non-invoiced/total
    [HttpGet("client/{clientId}/non-invoiced/total")]
    public async Task<ActionResult<decimal>> GetNonInvoicedTotalByClient(int clientId)
    {
        var total = await _context.CreditTransactions
            .Where(t => t.ClientID == clientId && !t.Facture)
            .SumAsync(t => t.MontantTotal);

        return Ok(total);
    }

    // GET: api/CreditTransactions/client/5/available/total
    [HttpGet("client/{clientId}/available/total")]
    public async Task<ActionResult<decimal>> GetAvailableTotalByClient(int clientId)
    {
        var total = await _context.CreditTransactions
            .Where(t => t.ClientID == clientId && !t.EstEnBL && !t.Facture)
            .SumAsync(t => t.MontantTotal);

        return Ok(total);
    }

    // ════════════════════════════════════════════════════════════════
    // CARBURANT CREDIT TRANSACTIONS FOR PERIODE
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get carburant credit transactions within a date range that are not yet assigned to a période.
    /// Used when creating a new période to show available credits.
    /// </summary>
    /// <param name="start">Start date/time (inclusive) - only the DATE portion is used</param>
    /// <param name="end">End date/time (inclusive) - only the DATE portion is used</param>
    /// <remarks>
    /// Since CreditTransactions are created with only a date (time defaults to midnight),
    /// while Periodes have specific start/end times, we compare dates only.
    /// This ensures a CT created on 2026-01-01 is included in a Periode spanning 
    /// 2026-01-01 10:00 to 2026-01-02 10:00.
    /// </remarks>
    [HttpGet("carburant/date-range")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetCarburantByDateRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        // Extract just the DATE portion for comparison (ignore time)
        // This ensures CTs with DateCredit at any time on the start/end dates are included
        var startDate = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(end.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc); // End of day

        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
                .ThenInclude(p => p!.Categorie)
            .Where(t => t.DateCredit >= startDate && t.DateCredit <= endDate)
            .Where(t => t.PeriodeID == null) // Not yet assigned to a période
            .Where(t => t.ProduitID != null && t.Produit!.CategorieID == 1) // Carburant category (ID=1)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    /// <summary>
    /// Get carburant credit transactions assigned to a specific période.
    /// Used when editing an existing période.
    /// </summary>
    /// <param name="periodeId">The période ID</param>
    [HttpGet("periode/{periodeId}")]
    public async Task<ActionResult<List<CreditTransactionDto>>> GetByPeriode(int periodeId)
    {
        var transactions = await _context.CreditTransactions
            .Include(t => t.Client)
            .Include(t => t.Produit)
            .Where(t => t.PeriodeID == periodeId)
            .AsNoTracking()
            .OrderByDescending(t => t.DateCredit)
            .ToListAsync();

        var dtos = _mapper.Map<List<CreditTransactionDto>>(transactions);
        return Ok(dtos);
    }

    /// <summary>
    /// Batch update PeriodeID for multiple credit transactions.
    /// Used when creating/updating a période to link CTs.
    /// </summary>
    /// <param name="periodeId">The période ID (or null to unlink)</param>
    /// <param name="creditIds">List of credit transaction IDs</param>
    [HttpPut("batch/link-periode/{periodeId:int?}")]
    public async Task<IActionResult> BatchLinkToPeriode(int? periodeId, [FromBody] List<int> creditIds)
    {
        if (creditIds == null || creditIds.Count == 0)
            return BadRequest("No credit transaction IDs provided");

        var transactions = await _context.CreditTransactions
            .Where(t => creditIds.Contains(t.CreditID))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.PeriodeID = periodeId;
            transaction.DateModification = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { Updated = transactions.Count, PeriodeID = periodeId });
    }

    /// <summary>
    /// Unlink credit transactions from a période (set PeriodeID to null).
    /// </summary>
    [HttpPut("batch/unlink-periode")]
    public async Task<IActionResult> BatchUnlinkFromPeriode([FromBody] List<int> creditIds)
    {
        return await BatchLinkToPeriode(null, creditIds);
    }

    // PUT: api/CreditTransactions/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCreditTransaction(int id, [FromBody] CreditTransactionDto dto)
    {
        if (id != dto.CreditID)
            return BadRequest("ID mismatch.");

        var existing = await _context.CreditTransactions.FindAsync(id);
        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);
        
        existing.DateModification = DateTime.UtcNow;
        existing.DateCredit = DateTime.SpecifyKind(existing.DateCredit, DateTimeKind.Utc);
        
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
            if (!CreditTransactionExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // PUT: api/CreditTransactions/5/mark-invoiced/10
    [HttpPut("{id}/mark-invoiced/{factureId}")]
    public async Task<IActionResult> MarkAsInvoiced(int id, int factureId)
    {
        var transaction = await _context.CreditTransactions.FindAsync(id);
        if (transaction == null)
            return NotFound();

        transaction.Facture = true;
        transaction.FactureID = factureId;
        transaction.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT: api/CreditTransactions/5/mark-in-bl/10
    [HttpPut("{id}/mark-in-bl/{bonLivraisonId}")]
    public async Task<IActionResult> MarkAsInBL(int id, int bonLivraisonId)
    {
        var transaction = await _context.CreditTransactions.FindAsync(id);
        if (transaction == null)
            return NotFound();

        transaction.EstEnBL = true;
        transaction.BonLivraisonID = bonLivraisonId;
        transaction.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT: api/CreditTransactions/batch/mark-in-bl/10
    [HttpPut("batch/mark-in-bl/{bonLivraisonId}")]
    public async Task<IActionResult> BatchMarkAsInBL(int bonLivraisonId, [FromBody] List<int> creditIds)
    {
        var transactions = await _context.CreditTransactions
            .Where(t => creditIds.Contains(t.CreditID))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.EstEnBL = true;
            transaction.BonLivraisonID = bonLivraisonId;
            transaction.DateModification = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { Updated = transactions.Count });
    }

    // PUT: api/CreditTransactions/batch/mark-invoiced/10
    [HttpPut("batch/mark-invoiced/{factureId}")]
    public async Task<IActionResult> BatchMarkAsInvoiced(int factureId, [FromBody] List<int> creditIds)
    {
        var transactions = await _context.CreditTransactions
            .Where(t => creditIds.Contains(t.CreditID))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.Facture = true;
            transaction.FactureID = factureId;
            transaction.DateModification = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { Updated = transactions.Count });
    }

    // POST: api/CreditTransactions
    [HttpPost]
    public async Task<ActionResult<CreditTransactionDto>> PostCreditTransaction([FromBody] CreditTransactionDto dto)
    {
        try
        {
            var entity = _mapper.Map<CreditTransaction>(dto);
            
            entity.DateCreation = DateTime.UtcNow;
            entity.DateCredit = DateTime.SpecifyKind(entity.DateCredit, DateTimeKind.Utc);
            
            if (entity.DateModification.HasValue)
            {
                entity.DateModification = DateTime.SpecifyKind(entity.DateModification.Value, DateTimeKind.Utc);
            }

            if (entity.MontantTotal == 0)
            {
                entity.MontantTotal = entity.PrixTTC * entity.Quantite;
            }

            _context.CreditTransactions.Add(entity);
            await _context.SaveChangesAsync();

            var createdEntity = await _context.CreditTransactions
                .Include(t => t.Client)
                .Include(t => t.Produit)
                .Include(t => t.Service)
                .FirstOrDefaultAsync(t => t.CreditID == entity.CreditID);

            var resultDto = _mapper.Map<CreditTransactionDto>(createdEntity);
            return CreatedAtAction(nameof(GetCreditTransaction), new { id = entity.CreditID }, resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // DELETE: api/CreditTransactions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCreditTransaction(int id)
    {
        var transaction = await _context.CreditTransactions.FindAsync(id);
        if (transaction == null)
            return NotFound();

        _context.CreditTransactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CreditTransactionExists(int id)
    {
        return _context.CreditTransactions.Any(e => e.CreditID == id);
    }
}
