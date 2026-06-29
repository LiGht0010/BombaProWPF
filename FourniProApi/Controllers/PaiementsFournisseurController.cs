using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaiementsFournisseurController(
    AppDbContext context,
    IMapper mapper,
    ILogger<PaiementsFournisseurController> logger)
    : ControllerBase
{
    // GET: api/PaiementsFournisseur?creditFournisseurId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaiementFournisseurDto>>> GetPaiements(
        [FromQuery] int? creditFournisseurId)
    {
        var query = context.PaiementsFournisseur.AsNoTracking();

        if (creditFournisseurId.HasValue)
            query = query.Where(p => p.CreditFournisseurId == creditFournisseurId.Value);

        var paiements = await query
            .OrderByDescending(p => p.DatePaiement)
            .ToListAsync();

        var dtos = mapper.Map<List<PaiementFournisseurDto>>(paiements);
        await ResolveNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/PaiementsFournisseur/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaiementFournisseurDto>> GetPaiement(int id)
    {
        var paiement = await context.PaiementsFournisseur
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaiementFournisseurId == id);

        if (paiement is null) return NotFound();

        var dto = mapper.Map<PaiementFournisseurDto>(paiement);
        await ResolveNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/PaiementsFournisseur
    [HttpPost]
    public async Task<ActionResult<PaiementFournisseurDto>> CreatePaiement(PaiementFournisseurDto dto)
    {
        var credit = await context.CreditsFournisseur
            .FirstOrDefaultAsync(c => c.CreditFournisseurId == dto.CreditFournisseurId);

        if (credit is null)
            return BadRequest($"CreditFournisseur {dto.CreditFournisseurId} not found.");

        var paiement = mapper.Map<PaiementFournisseur>(dto);
        paiement.DateCreation     = DateTime.UtcNow;
        paiement.DateModification = DateTime.UtcNow;

        context.PaiementsFournisseur.Add(paiement);
        await context.SaveChangesAsync();

        await RecalcStatutAsync(credit);
        await context.SaveChangesAsync();

        var result = mapper.Map<PaiementFournisseurDto>(paiement);
        await ResolveNamesAsync([result]);
        return CreatedAtAction(nameof(GetPaiement),
            new { id = paiement.PaiementFournisseurId }, result);
    }

    // PUT: api/PaiementsFournisseur/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PaiementFournisseurDto>> UpdatePaiement(
        int id, PaiementFournisseurDto dto)
    {
        var paiement = await context.PaiementsFournisseur
            .FirstOrDefaultAsync(p => p.PaiementFournisseurId == id);

        if (paiement is null) return NotFound();

        mapper.Map(dto, paiement);
        paiement.PaiementFournisseurId = id;
        paiement.DateModification      = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var credit = await context.CreditsFournisseur
            .FirstOrDefaultAsync(c => c.CreditFournisseurId == paiement.CreditFournisseurId);

        if (credit is not null)
        {
            await RecalcStatutAsync(credit);
            await context.SaveChangesAsync();
        }

        var result = mapper.Map<PaiementFournisseurDto>(paiement);
        await ResolveNamesAsync([result]);
        return Ok(result);
    }

    // DELETE: api/PaiementsFournisseur/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePaiement(int id)
    {
        var paiement = await context.PaiementsFournisseur
            .FirstOrDefaultAsync(p => p.PaiementFournisseurId == id);

        if (paiement is null) return NotFound();

        int creditId = paiement.CreditFournisseurId;
        context.PaiementsFournisseur.Remove(paiement);
        await context.SaveChangesAsync();

        var credit = await context.CreditsFournisseur
            .FirstOrDefaultAsync(c => c.CreditFournisseurId == creditId);

        if (credit is not null)
        {
            await RecalcStatutAsync(credit);
            await context.SaveChangesAsync();
        }

        return NoContent();
    }

    // ── Business logic ────────────────────────────────────────────────────────

    /// <summary>
    /// Recomputes <see cref="CreditFournisseur.Statut"/> and <see cref="CreditFournisseur.StatutCheque"/>
    /// based on the sum of all paiements for the credit.
    ///
    /// Rules:
    ///   SUM == 0               → NonPayé
    ///   0 &lt; SUM &lt; MontantTotal → PartielPayé
    ///   SUM &gt;= MontantTotal     → Payé
    ///
    /// StatutCheque when Payé:
    ///   Any paiement used ChequeEncaisse → Déposé
    ///   Otherwise (normal payments)      → Retourné
    /// StatutCheque is left unchanged while PartielPayé/NonPayé.
    /// </summary>
    private async Task RecalcStatutAsync(CreditFournisseur credit)
    {
        var paiements = await context.PaiementsFournisseur
            .Where(p => p.CreditFournisseurId == credit.CreditFournisseurId)
            .ToListAsync();

        var totalPaye    = paiements.Sum(p => p.Montant ?? 0m);
        var montantTotal = credit.MontantTotal ?? 0m;

        string newStatut;
        if (totalPaye <= 0m)
            newStatut = nameof(CreditFournisseurStatut.NonPayé);
        else if (totalPaye < montantTotal)
            newStatut = nameof(CreditFournisseurStatut.PartielPayé);
        else
            newStatut = nameof(CreditFournisseurStatut.Payé);

        credit.Statut = newStatut;

        // Only update StatutCheque when fully settled AND a cheque was recorded
        if (newStatut == nameof(CreditFournisseurStatut.Payé)
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
            "CreditFournisseur {Id} recalculated → Statut={Statut}, TotalPayé={Total:N2}",
            credit.CreditFournisseurId, credit.Statut, totalPaye);
    }

    // ── Name resolution ───────────────────────────────────────────────────────

    private async Task ResolveNamesAsync(IList<PaiementFournisseurDto> dtos)
    {
        var creditIds = dtos.Select(d => d.CreditFournisseurId).Distinct().ToList();
        var credits = await context.CreditsFournisseur
            .AsNoTracking()
            .Where(c => creditIds.Contains(c.CreditFournisseurId))
            .Select(c => new { c.CreditFournisseurId, c.NumeroCreditF })
            .ToDictionaryAsync(c => c.CreditFournisseurId);

        var employeIds = dtos
            .Where(d => d.EmployeId.HasValue)
            .Select(d => d.EmployeId!.Value)
            .Distinct().ToList();
        var employes = await context.Employes
            .AsNoTracking()
            .Where(e => employeIds.Contains(e.EmployeId))
            .Select(e => new { e.EmployeId, Nom = e.Prenom + " " + e.Nom })
            .ToDictionaryAsync(e => e.EmployeId);

        var userIds = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct().ToList();
        var users = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.Name })
            .ToDictionaryAsync(u => u.UserId);

        foreach (var dto in dtos)
        {
            if (credits.TryGetValue(dto.CreditFournisseurId, out var c))
                dto.NumeroCreditF = c.NumeroCreditF;

            if (dto.EmployeId.HasValue && employes.TryGetValue(dto.EmployeId.Value, out var e))
                dto.EmployeNom = e.Nom;

            if (dto.AjoutePar.HasValue && users.TryGetValue(dto.AjoutePar.Value, out var ua))
                dto.AjouteParNom = ua.Name;

            if (dto.ModifiePar.HasValue && users.TryGetValue(dto.ModifiePar.Value, out var um))
                dto.ModifieParNom = um.Name;
        }
    }
}
