using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FacturesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public FacturesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Factures
    [HttpGet]
    public async Task<ActionResult<List<FactureDto>>> GetFactures()
    {
        var factures = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .AsNoTracking()
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();

        var dtos = _mapper.Map<List<FactureDto>>(factures);
        return Ok(dtos);
    }

    // GET: api/Factures/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FactureDto>> GetFacture(int id)
    {
        var facture = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.ID == id);

        if (facture == null)
            return NotFound();

        var dto = _mapper.Map<FactureDto>(facture);
        return Ok(dto);
    }

    // GET: api/Factures/5/details
    [HttpGet("{id}/details")]
    public async Task<ActionResult<FactureDto>> GetFactureWithDetails(int id)
    {
        var facture = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .Include(f => f.ElementsFactures)
                .ThenInclude(e => e.Produit)
            .Include(f => f.ElementsFactures)
                .ThenInclude(e => e.Service)
            .Include(f => f.FactureBonLivraisons)
                .ThenInclude(fbl => fbl.BonLivraison)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.ID == id);

        if (facture == null)
            return NotFound();

        var dto = _mapper.Map<FactureDto>(facture);
        return Ok(dto);
    }

    // GET: api/Factures/numero/FAC-2025-00001
    [HttpGet("numero/{numeroFacture}")]
    public async Task<ActionResult<FactureDto>> GetFactureByNumero(string numeroFacture)
    {
        var facture = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.NumeroFacture == numeroFacture);

        if (facture == null)
            return NotFound();

        var dto = _mapper.Map<FactureDto>(facture);
        return Ok(dto);
    }

    // GET: api/Factures/client/5
    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<List<FactureDto>>> GetFacturesByClient(int clientId)
    {
        var factures = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .Where(f => f.ClientID == clientId)
            .AsNoTracking()
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();

        var dtos = _mapper.Map<List<FactureDto>>(factures);
        return Ok(dtos);
    }

    // GET: api/Factures/statut/Non%20Payée
    [HttpGet("statut/{statut}")]
    public async Task<ActionResult<List<FactureDto>>> GetFacturesByStatut(string statut)
    {
        var factures = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .Where(f => f.Statut == statut)
            .AsNoTracking()
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();

        var dtos = _mapper.Map<List<FactureDto>>(factures);
        return Ok(dtos);
    }

    // GET: api/Factures/date-range?start=2025-01-01&end=2025-12-31
    [HttpGet("date-range")]
    public async Task<ActionResult<List<FactureDto>>> GetFacturesByDateRange(
        [FromQuery] DateOnly start,
        [FromQuery] DateOnly end)
    {
        var factures = await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.MoyenPaiement)
            .Where(f => f.DateFacture >= start && f.DateFacture <= end)
            .AsNoTracking()
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();

        var dtos = _mapper.Map<List<FactureDto>>(factures);
        return Ok(dtos);
    }

    // GET: api/Factures/next-numero
    [HttpGet("next-numero")]
    public async Task<ActionResult<string>> GetNextNumeroFacture()
    {
        var numero = await GenerateNextNumeroFactureAsync();
        return Ok(numero);
    }

    // PUT: api/Factures/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFacture(int id, [FromBody] FactureDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch.");

        var existing = await _context.Factures.FindAsync(id);
        if (existing == null)
            return NotFound();

        // Update properties
        existing.NumeroFacture = dto.NumeroFacture;
        existing.DateFacture = dto.DateFacture;
        existing.ClientID = dto.ClientID;
        existing.MontantTotal = dto.MontantTotal;
        existing.Statut = dto.Statut;
        existing.MoyenPaiementID = dto.MoyenPaiementID;
        existing.DatePaiement = dto.DatePaiement;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FactureExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // PUT: api/Factures/5/statut
    [HttpPut("{id}/statut")]
    public async Task<IActionResult> UpdateFactureStatut(int id, [FromBody] string statut)
    {
        var facture = await _context.Factures.FindAsync(id);
        if (facture == null)
            return NotFound();

        facture.Statut = statut;

        if (statut == "Payée" && !facture.DatePaiement.HasValue)
        {
            facture.DatePaiement = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Factures
    [HttpPost]
    public async Task<ActionResult<FactureDto>> PostFacture([FromBody] FactureDto dto)
    {
        try
        {
            // Generate facture number if not provided
            if (string.IsNullOrWhiteSpace(dto.NumeroFacture))
            {
                dto.NumeroFacture = await GenerateNextNumeroFactureAsync();
            }

            // Check for duplicate facture number
            if (await _context.Factures.AnyAsync(f => f.NumeroFacture == dto.NumeroFacture))
            {
                return BadRequest($"Une facture avec le numéro '{dto.NumeroFacture}' existe déjà.");
            }

            var facture = new Facture
            {
                NumeroFacture = dto.NumeroFacture,
                DateFacture = dto.DateFacture ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ClientID = dto.ClientID,
                MontantTotal = dto.MontantTotal ?? 0,
                Statut = "Payée", // Facture is always created as paid
                MoyenPaiementID = dto.MoyenPaiementID,
                DatePaiement = dto.DatePaiement ?? DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdEntity = await _context.Factures
                .Include(f => f.Client)
                .Include(f => f.MoyenPaiement)
                .FirstOrDefaultAsync(f => f.ID == facture.ID);

            var resultDto = _mapper.Map<FactureDto>(createdEntity);
            return CreatedAtAction(nameof(GetFacture), new { id = facture.ID }, resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // POST: api/Factures/from-credit-transactions (⭐ NEW - Direct invoice from CTs)
    [HttpPost("from-credit-transactions")]
    public async Task<ActionResult<CTConversionResultDto>> CreateFactureFromCreditTransactions([FromBody] CreateFactureFromCTsDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate input
            if (dto.CreditTransactionIds == null || !dto.CreditTransactionIds.Any())
            {
                return BadRequest(new CTConversionResultDto
                {
                    Success = false,
                    Message = "Aucune transaction crédit sélectionnée.",
                    Errors = ["CreditTransactionIds est vide"]
                });
            }

            // Load CTs
            var creditTransactions = await _context.CreditTransactions
                .Include(ct => ct.Client)
                .Include(ct => ct.Produit)
                .Include(ct => ct.Service)
                .Where(ct => dto.CreditTransactionIds.Contains(ct.CreditID))
                .ToListAsync();

            // Validate all CTs found
            if (creditTransactions.Count != dto.CreditTransactionIds.Count)
            {
                var foundIds = creditTransactions.Select(ct => ct.CreditID).ToList();
                var missingIds = dto.CreditTransactionIds.Except(foundIds).ToList();
                return BadRequest(new CTConversionResultDto
                {
                    Success = false,
                    Message = "Certaines transactions crédit n'existent pas.",
                    Errors = missingIds.Select(id => $"CT ID {id} non trouvé").ToList()
                });
            }

            // Validate all CTs belong to same client
            var clientIds = creditTransactions.Select(ct => ct.ClientID).Distinct().ToList();
            if (clientIds.Count > 1)
            {
                return BadRequest(new CTConversionResultDto
                {
                    Success = false,
                    Message = "Toutes les transactions doivent appartenir au même client.",
                    Errors = ["Clients multiples détectés"]
                });
            }

            // Validate none are already invoiced
            var alreadyInvoiced = creditTransactions.Where(ct => ct.Facture).ToList();
            if (alreadyInvoiced.Any())
            {
                return BadRequest(new CTConversionResultDto
                {
                    Success = false,
                    Message = "Certaines transactions sont déjà facturées.",
                    Errors = alreadyInvoiced.Select(ct => $"CT {ct.NumeroTransaction} déjà facturé").ToList()
                });
            }

            // Create Facture
            var numeroFacture = await GenerateNextNumeroFactureAsync();
            var clientId = dto.ClientID > 0 ? dto.ClientID : clientIds.First();

            var facture = new Facture
            {
                NumeroFacture = numeroFacture,
                DateFacture = dto.DateFacture ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ClientID = clientId,
                Statut = "Payée", // Facture is always created as paid
                MoyenPaiementID = dto.MoyenPaiementID,
                DatePaiement = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Create Facture elements from CTs
            decimal montantTotal = 0;
            foreach (var ct in creditTransactions)
            {
                var element = new ElementsFacture
                {
                    ProduitID = ct.ProduitID,
                    ServiceID = ct.ServiceID,
                    Quantite = ct.Quantite,
                    PrixUnitaire = ct.PrixTTC
                };
                facture.ElementsFactures.Add(element);
                montantTotal += ct.MontantTotal;
            }

            facture.MontantTotal = montantTotal;

            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();

            // Mark CTs as invoiced
            foreach (var ct in creditTransactions)
            {
                ct.Facture = true;
                ct.FactureID = facture.ID;
                ct.DateModification = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new CTConversionResultDto
            {
                Success = true,
                Message = $"Facture {numeroFacture} créée avec succès.",
                FactureID = facture.ID,
                NumeroFacture = numeroFacture,
                MontantTotal = montantTotal,
                CTsConverted = creditTransactions.Count
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new CTConversionResultDto
            {
                Success = false,
                Message = "Erreur lors de la création de la facture.",
                Errors = [ex.Message, ex.InnerException?.Message ?? ""]
            });
        }
    }

    // POST: api/Factures/from-bons-livraison (⭐ Create Facture from BLs)
    [HttpPost("from-bons-livraison")]
    public async Task<ActionResult<FacturationResultDto>> CreateFactureFromBonsLivraison([FromBody] CreateFactureFromBLsDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate input
            if (dto.BonLivraisonIds == null || dto.BonLivraisonIds.Count == 0)
            {
                return BadRequest(new FacturationResultDto
                {
                    Success = false,
                    Message = "Aucun bon de livraison sélectionné.",
                    Errors = ["BonLivraisonIds est vide"]
                });
            }

            // Load BLs with details and related CTs
            var bonsLivraison = await _context.BonsLivraison
                .Include(bl => bl.Client)
                .Include(bl => bl.Details)
                    .ThenInclude(d => d.Produit)
                .Include(bl => bl.Details)
                    .ThenInclude(d => d.Service)
                .Include(bl => bl.CreditTransactions)
                .Where(bl => dto.BonLivraisonIds.Contains(bl.ID))
                .ToListAsync();

            // Validate all BLs found
            if (bonsLivraison.Count != dto.BonLivraisonIds.Count)
            {
                var foundIds = bonsLivraison.Select(bl => bl.ID).ToList();
                var missingIds = dto.BonLivraisonIds.Except(foundIds).ToList();
                return BadRequest(new FacturationResultDto
                {
                    Success = false,
                    Message = "Certains bons de livraison n'existent pas.",
                    Errors = missingIds.Select(id => $"BL ID {id} non trouvé").ToList()
                });
            }

            // Validate all BLs belong to same client
            var clientIds = bonsLivraison.Select(bl => bl.ClientID).Distinct().ToList();
            if (clientIds.Count > 1)
            {
                return BadRequest(new FacturationResultDto
                {
                    Success = false,
                    Message = "Tous les bons de livraison doivent appartenir au même client.",
                    Errors = ["Clients multiples détectés"]
                });
            }

            // Validate none are already invoiced
            var alreadyInvoiced = bonsLivraison.Where(bl => bl.EstFacture).ToList();
            if (alreadyInvoiced.Any())
            {
                return BadRequest(new FacturationResultDto
                {
                    Success = false,
                    Message = "Certains bons de livraison sont déjà facturés.",
                    Errors = alreadyInvoiced.Select(bl => $"BL {bl.NumeroBL} déjà facturé").ToList()
                });
            }

            // Create Facture
            var numeroFacture = await GenerateNextNumeroFactureAsync();
            var clientId = dto.ClientID > 0 ? dto.ClientID : clientIds.First();

            var facture = new Facture
            {
                NumeroFacture = numeroFacture,
                DateFacture = dto.DateFacture ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ClientID = clientId,
                Statut = "Payée", // Facture is always created as paid
                MoyenPaiementID = dto.MoyenPaiementID,
                DatePaiement = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Create Facture elements from BL details
            decimal montantTotal = 0;
            foreach (var bl in bonsLivraison)
            {
                foreach (var detail in bl.Details)
                {
                    var element = new ElementsFacture
                    {
                        ProduitID = detail.ProduitID,
                        ServiceID = detail.ServiceID,
                        Quantite = detail.Quantite,
                        PrixUnitaire = detail.PrixUnitaire
                    };
                    facture.ElementsFactures.Add(element);
                    montantTotal += detail.MontantLigne;
                }
            }

            facture.MontantTotal = montantTotal;

            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();

            // Create junction records (FactureBonLivraison) and mark BLs as invoiced
            foreach (var bl in bonsLivraison)
            {
                // Create junction record
                var junction = new FactureBonLivraison
                {
                    FactureID = facture.ID,
                    BonLivraisonID = bl.ID
                };
                _context.FactureBonLivraisons.Add(junction);

                // Mark BL as invoiced
                bl.EstFacture = true;
                bl.DateModification = DateTime.UtcNow;

                // Mark related CTs as invoiced
                foreach (var ct in bl.CreditTransactions)
                {
                    ct.Facture = true;
                    ct.FactureID = facture.ID;
                    ct.DateModification = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new FacturationResultDto
            {
                Success = true,
                Message = $"Facture {numeroFacture} créée avec succès à partir de {bonsLivraison.Count} BL(s).",
                FactureID = facture.ID,
                NumeroFacture = numeroFacture,
                MontantTotal = montantTotal,
                BLsFactures = bonsLivraison.Count
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new FacturationResultDto
            {
                Success = false,
                Message = "Erreur lors de la création de la facture.",
                Errors = [ex.Message, ex.InnerException?.Message ?? ""]
            });
        }
    }

    // POST: api/Factures/merge (⭐ NEW - Merge multiple Factures into one)
    [HttpPost("merge")]
    public async Task<ActionResult<MergeFacturesResultDto>> MergeFactures([FromBody] MergeFacturesDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate input
            if (dto.FactureIds == null || dto.FactureIds.Count < 2)
            {
                return BadRequest(new MergeFacturesResultDto
                {
                    Success = false,
                    Message = "Veuillez sélectionner au moins 2 factures à fusionner.",
                    Errors = new List<string> { "FactureIds doit contenir au moins 2 IDs" }
                });
            }

            // Load Factures with related data
            var factures = await _context.Factures
                .Include(f => f.ElementsFactures)
                .Include(f => f.FactureBonLivraisons)
                    .ThenInclude(fbl => fbl.BonLivraison)
                .Include(f => f.CreditTransactions)
                .Where(f => dto.FactureIds.Contains(f.ID))
                .ToListAsync();

            // Validate all Factures found
            if (factures.Count != dto.FactureIds.Count)
            {
                var foundIds = factures.Select(f => f.ID).ToList();
                var missingIds = dto.FactureIds.Except(foundIds).ToList();
                return BadRequest(new MergeFacturesResultDto
                {
                    Success = false,
                    Message = "Certaines factures n'existent pas.",
                    Errors = missingIds.Select(id => $"Facture ID {id} non trouvée").ToList()
                });
            }

            // Validate all Factures belong to same client
            var clientIds = factures.Select(f => f.ClientID).Distinct().ToList();
            if (clientIds.Count > 1)
            {
                return BadRequest(new MergeFacturesResultDto
                {
                    Success = false,
                    Message = "Toutes les factures doivent appartenir au même client.",
                    Errors = new List<string> { "Clients multiples détectés" }
                });
            }

            // Create new merged Facture
            var numeroFacture = await GenerateNextNumeroFactureAsync();
            var clientId = dto.ClientID > 0 ? dto.ClientID : clientIds.First()!.Value;

            var newFacture = new Facture
            {
                NumeroFacture = numeroFacture,
                DateFacture = dto.DateFacture ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ClientID = clientId,
                Statut = "Payée", // Facture is always created as paid
                MoyenPaiementID = dto.MoyenPaiementID,
                DatePaiement = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Merge all elements from source Factures
            decimal montantTotal = 0;
            foreach (var sourceFacture in factures)
            {
                foreach (var element in sourceFacture.ElementsFactures)
                {
                    var newElement = new ElementsFacture
                    {
                        ProduitID = element.ProduitID,
                        ServiceID = element.ServiceID,
                        Quantite = element.Quantite,
                        PrixUnitaire = element.PrixUnitaire
                    };
                    newFacture.ElementsFactures.Add(newElement);
                    montantTotal += (element.Quantite ?? 0) * (element.PrixUnitaire ?? 0);
                }
            }

            newFacture.MontantTotal = montantTotal;

            _context.Factures.Add(newFacture);
            await _context.SaveChangesAsync();

            // Transfer credit transactions to new Facture
            foreach (var sourceFacture in factures)
            {
                foreach (var ct in sourceFacture.CreditTransactions)
                {
                    ct.FactureID = newFacture.ID;
                    ct.DateModification = DateTime.UtcNow;
                }

                // Transfer BL links
                foreach (var fbl in sourceFacture.FactureBonLivraisons)
                {
                    var newLink = new FactureBonLivraison
                    {
                        FactureID = newFacture.ID,
                        BonLivraisonID = fbl.BonLivraisonID
                    };
                    _context.FactureBonLivraisons.Add(newLink);
                }
            }

            // Delete old Factures
            foreach (var oldFacture in factures)
            {
                _context.FactureBonLivraisons.RemoveRange(oldFacture.FactureBonLivraisons);
                _context.ElementsFactures.RemoveRange(oldFacture.ElementsFactures);
                _context.Factures.Remove(oldFacture);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new MergeFacturesResultDto
            {
                Success = true,
                Message = $"Facture consolidée {numeroFacture} créée avec succès.",
                NewFactureID = newFacture.ID,
                NewNumeroFacture = numeroFacture,
                MontantTotal = montantTotal,
                FacturesMerged = factures.Count
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new MergeFacturesResultDto
            {
                Success = false,
                Message = "Erreur lors de la fusion des factures.",
                Errors = new List<string> { ex.Message, ex.InnerException?.Message ?? "" }
            });
        }
    }

    // DELETE: api/Factures/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFacture(int id)
    {
        var facture = await _context.Factures
            .Include(f => f.ElementsFactures)
            .Include(f => f.FactureBonLivraisons)
                .ThenInclude(fbl => fbl.BonLivraison)
            .Include(f => f.CreditTransactions)
            .FirstOrDefaultAsync(f => f.ID == id);

        if (facture == null)
            return NotFound();

        // Unlock associated BLs (set EstFacture = false)
        foreach (var fbl in facture.FactureBonLivraisons)
        {
            if (fbl.BonLivraison != null)
            {
                fbl.BonLivraison.EstFacture = false;
                fbl.BonLivraison.DateModification = DateTime.UtcNow;
            }
        }

        // Unlink CreditTransactions (set Facture = false, FactureID = null)
        foreach (var ct in facture.CreditTransactions)
        {
            ct.Facture = false;
            ct.FactureID = null;
            ct.DateModification = DateTime.UtcNow;
        }

        // Remove junction records
        _context.FactureBonLivraisons.RemoveRange(facture.FactureBonLivraisons);

        // Remove elements
        _context.ElementsFactures.RemoveRange(facture.ElementsFactures);

        // Remove facture
        _context.Factures.Remove(facture);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool FactureExists(int id)
    {
        return _context.Factures.Any(e => e.ID == id);
    }

    private async Task<string> GenerateNextNumeroFactureAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"FAC-{currentYear}-";

        var lastFacture = await _context.Factures
            .Where(f => f.NumeroFacture != null && f.NumeroFacture.StartsWith(prefix))
            .OrderByDescending(f => f.NumeroFacture)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastFacture?.NumeroFacture != null)
        {
            var numberPart = lastFacture.NumeroFacture.Replace(prefix, "");
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D5}"; // e.g., FAC-2025-00001
    }
}
