using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaiementsCreditController(
    AppDbContext context,
    IMapper mapper,
    ILogger<PaiementsCreditController> logger)
    : ControllerBase
{
    // GET: api/PaiementsCredit?creditId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaiementCreditDto>>> GetPaiements(
        [FromQuery] int? creditId)
    {
        var query = context.PaiementsCredit.AsNoTracking();

        if (creditId.HasValue)
            query = query.Where(p => p.CreditId == creditId.Value);

        var paiements = await query
            .OrderByDescending(p => p.DatePaiement)
            .ToListAsync();

        var dtos = mapper.Map<List<PaiementCreditDto>>(paiements);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/PaiementsCredit/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaiementCreditDto>> GetPaiement(int id)
    {
        var paiement = await context.PaiementsCredit
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaiementCreditId == id);

        if (paiement is null) return NotFound();

        var dto = mapper.Map<PaiementCreditDto>(paiement);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/PaiementsCredit
    [HttpPost]
    public async Task<ActionResult<PaiementCreditDto>> CreatePaiement(PaiementCreditDto dto)
    {
        var credit = await context.Credits
            .FirstOrDefaultAsync(c => c.CreditId == dto.CreditId);

        if (credit is null)
            return BadRequest($"Credit {dto.CreditId} not found.");

        var paiement = mapper.Map<PaiementCredit>(dto);
        paiement.DateCreation     = DateTime.UtcNow;
        paiement.DateModification = DateTime.UtcNow;

        context.PaiementsCredit.Add(paiement);
        await context.SaveChangesAsync();

        await RecalcCreditStatutAsync(credit);
        await context.SaveChangesAsync();

        var result = mapper.Map<PaiementCreditDto>(paiement);
        await ResolveNamesAsync([result]);
        return CreatedAtAction(nameof(GetPaiement),
            new { id = paiement.PaiementCreditId }, result);
    }

    // PUT: api/PaiementsCredit/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PaiementCreditDto>> UpdatePaiement(
        int id, PaiementCreditDto dto)
    {
        var paiement = await context.PaiementsCredit
            .FirstOrDefaultAsync(p => p.PaiementCreditId == id);

        if (paiement is null) return NotFound();

        mapper.Map(dto, paiement);
        paiement.PaiementCreditId = id;
        paiement.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var credit = await context.Credits
            .FirstOrDefaultAsync(c => c.CreditId == paiement.CreditId);

        if (credit is not null)
        {
            await RecalcCreditStatutAsync(credit);
            await context.SaveChangesAsync();
        }

        var result = mapper.Map<PaiementCreditDto>(paiement);
        await ResolveNamesAsync([result]);
        return Ok(result);
    }

    // DELETE: api/PaiementsCredit/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePaiement(int id)
    {
        var paiement = await context.PaiementsCredit
            .FirstOrDefaultAsync(p => p.PaiementCreditId == id);

        if (paiement is null) return NotFound();

        int creditId = paiement.CreditId;
        context.PaiementsCredit.Remove(paiement);
        await context.SaveChangesAsync();

        var credit = await context.Credits
            .FirstOrDefaultAsync(c => c.CreditId == creditId);

        if (credit is not null)
        {
            await RecalcCreditStatutAsync(credit);
            await context.SaveChangesAsync();
        }

        return NoContent();
    }

    // ── Business logic ────────────────────────────────────────────────────────

    /// <summary>
    /// Recomputes <see cref="Credit.Statut"/> and <see cref="Credit.StatutCheque"/>
    /// based on the sum of all paiements for the credit.
    ///
    /// Rules:
    ///   SUM == 0               → Impayé
    ///   0 &lt; SUM &lt; MontantTotal → Partiel
    ///   SUM &gt;= MontantTotal     → Soldé
    ///
    /// StatutCheque when Soldé:
    ///   Any paiement used ChequeDepose → Déposé
    ///   Otherwise (normal payments)    → Retourné
    /// StatutCheque is left unchanged while Partiel/Impayé.
    /// </summary>
    private async Task RecalcCreditStatutAsync(Credit credit)
    {
        var paiements = await context.PaiementsCredit
            .Where(p => p.CreditId == credit.CreditId)
            .ToListAsync();

        var totalPaye = paiements.Sum(p => p.Montant ?? 0m);
        var montantTotal = credit.MontantTotal ?? 0m;

        string newStatut;
        if (totalPaye <= 0m)
            newStatut = nameof(CreditStatut.Impayé);
        else if (totalPaye < montantTotal)
            newStatut = nameof(CreditStatut.Partiel);
        else
            newStatut = nameof(CreditStatut.Soldé);

        credit.Statut = newStatut;

        // Only update StatutCheque when fully settled AND a cheque was recorded
        if (newStatut == nameof(CreditStatut.Soldé)
            && !string.IsNullOrWhiteSpace(credit.ChequeReference))
        {
            bool settledByCheque = paiements.Any(
                p => string.Equals(p.PaymentMethod,
                                   nameof(PaiementCreditMethod.ChequeEncaisse),
                                   StringComparison.OrdinalIgnoreCase));

            credit.StatutCheque = settledByCheque
                ? nameof(StatutCheque.Déposé)
                : nameof(StatutCheque.Retourné);
        }

        logger.LogInformation(
            "Credit {Id} recalculated → Statut={Statut}, TotalPayé={Total:N2}",
            credit.CreditId, credit.Statut, totalPaye);
    }

    // ── Name resolution ───────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<PaiementCreditDto> dtos)
    {
        var creditIds = dtos.Select(d => d.CreditId).Distinct().ToList();
        var credits = await context.Credits
            .AsNoTracking()
            .Where(c => creditIds.Contains(c.CreditId))
            .Select(c => new { c.CreditId, c.NumeroCredit })
            .ToDictionaryAsync(c => c.CreditId);

        var employeIds = dtos
            .Where(d => d.EmployeId.HasValue)
            .Select(d => d.EmployeId!.Value)
            .Distinct()
            .ToList();
        var employes = await context.Employes
            .AsNoTracking()
            .Where(e => employeIds.Contains(e.EmployeId))
            .Select(e => new { e.EmployeId, Nom = e.Prenom + " " + e.Nom })
            .ToDictionaryAsync(e => e.EmployeId);

        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var users = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.Name })
            .ToDictionaryAsync(u => u.UserId);

        foreach (var dto in dtos)
        {
            if (credits.TryGetValue(dto.CreditId, out var c))
                dto.NumeroCredit = c.NumeroCredit;

            if (dto.EmployeId.HasValue && employes.TryGetValue(dto.EmployeId.Value, out var e))
                dto.EmployeNom = e.Nom;

            if (dto.AjoutePar.HasValue && users.TryGetValue(dto.AjoutePar.Value, out var ua))
                dto.AjouteParNom = ua.Name;

            if (dto.ModifiePar.HasValue && users.TryGetValue(dto.ModifiePar.Value, out var um))
                dto.ModifieParNom = um.Name;
        }
    }
}
