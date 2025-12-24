using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BonLivraisonsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public BonLivraisonsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/BonLivraisons
    [HttpGet]
    public async Task<ActionResult<List<BonLivraisonDto>>> GetBonsLivraison()
    {
        var bonsLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .AsNoTracking()
            .OrderByDescending(bl => bl.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDto>>(bonsLivraison);
        return Ok(dtos);
    }

    // GET: api/BonLivraisons/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BonLivraisonDto>> GetBonLivraison(int id)
    {
        var bonLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(bl => bl.ID == id);

        if (bonLivraison == null)
            return NotFound();

        var dto = _mapper.Map<BonLivraisonDto>(bonLivraison);
        return Ok(dto);
    }

    // GET: api/BonLivraisons/numero/BL-2025-00001
    [HttpGet("numero/{numeroBL}")]
    public async Task<ActionResult<BonLivraisonDto>> GetBonLivraisonByNumero(string numeroBL)
    {
        var bonLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(bl => bl.NumeroBL == numeroBL);

        if (bonLivraison == null)
            return NotFound();

        var dto = _mapper.Map<BonLivraisonDto>(bonLivraison);
        return Ok(dto);
    }

    // GET: api/BonLivraisons/client/5
    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<List<BonLivraisonDto>>> GetBonsLivraisonByClient(int clientId)
    {
        var bonsLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .Where(bl => bl.ClientID == clientId)
            .AsNoTracking()
            .OrderByDescending(bl => bl.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDto>>(bonsLivraison);
        return Ok(dtos);
    }

    // GET: api/BonLivraisons/non-factures
    [HttpGet("non-factures")]
    public async Task<ActionResult<List<BonLivraisonDto>>> GetNonFacturedBonsLivraison()
    {
        var bonsLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .Where(bl => !bl.EstFacture)
            .AsNoTracking()
            .OrderByDescending(bl => bl.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDto>>(bonsLivraison);
        return Ok(dtos);
    }

    // GET: api/BonLivraisons/non-factures/client/5
    [HttpGet("non-factures/client/{clientId}")]
    public async Task<ActionResult<List<BonLivraisonDto>>> GetNonFacturedBonsLivraisonByClient(int clientId)
    {
        var bonsLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .Where(bl => bl.ClientID == clientId && !bl.EstFacture)
            .AsNoTracking()
            .OrderByDescending(bl => bl.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDto>>(bonsLivraison);
        return Ok(dtos);
    }

    // GET: api/BonLivraisons/date-range?start=2025-01-01&end=2025-12-31
    [HttpGet("date-range")]
    public async Task<ActionResult<List<BonLivraisonDto>>> GetBonsLivraisonByDateRange(
        [FromQuery] DateOnly start,
        [FromQuery] DateOnly end)
    {
        var bonsLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .Where(bl => bl.DateBL >= start && bl.DateBL <= end)
            .AsNoTracking()
            .OrderByDescending(bl => bl.DateBL)
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDto>>(bonsLivraison);
        return Ok(dtos);
    }

    // GET: api/BonLivraisons/next-numero
    [HttpGet("next-numero")]
    public async Task<ActionResult<string>> GetNextNumeroBL()
    {
        var numero = await GenerateNextNumeroBLAsync();
        return Ok(numero);
    }

    // POST: api/BonLivraisons
    [HttpPost]
    public async Task<ActionResult<BonLivraisonDto>> PostBonLivraison([FromBody] CreateBonLivraisonDto dto)
    {
        try
        {
            // Generate BL number if not provided
            if (string.IsNullOrWhiteSpace(dto.NumeroBL))
            {
                dto.NumeroBL = await GenerateNextNumeroBLAsync();
            }

            // Check for duplicate BL number
            if (await _context.BonsLivraison.AnyAsync(bl => bl.NumeroBL == dto.NumeroBL))
            {
                return BadRequest($"Un bon de livraison avec le numéro '{dto.NumeroBL}' existe déjà.");
            }

            // Validate client exists
            if (!await _context.Clients.AnyAsync(c => c.ID == dto.ClientID))
            {
                return BadRequest($"Le client avec l'ID {dto.ClientID} n'existe pas.");
            }

            var bonLivraison = _mapper.Map<BonLivraison>(dto);
            bonLivraison.DateCreation = DateTime.UtcNow;
            bonLivraison.EstFacture = false;

            // Create details and calculate totals
            decimal montantTotal = 0;
            foreach (var detailDto in dto.Details)
            {
                var detail = _mapper.Map<BonLivraisonDetails>(detailDto);
                detail.MontantLigne = detail.Quantite * detail.PrixUnitaire;
                montantTotal += detail.MontantLigne;
                bonLivraison.Details.Add(detail);
            }

            bonLivraison.MontantTotal = montantTotal;

            _context.BonsLivraison.Add(bonLivraison);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var createdEntity = await _context.BonsLivraison
                .Include(bl => bl.Client)
                .Include(bl => bl.Details)
                    .ThenInclude(d => d.Produit)
                .Include(bl => bl.Details)
                    .ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(bl => bl.ID == bonLivraison.ID);

            var resultDto = _mapper.Map<BonLivraisonDto>(createdEntity);
            return CreatedAtAction(nameof(GetBonLivraison), new { id = bonLivraison.ID }, resultDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }

    // POST: api/BonLivraisons/from-credit-transactions (? NEW - Create BL from CreditTransactions)
    [HttpPost("from-credit-transactions")]
    public async Task<ActionResult<CTConversionResultDto>> CreateBLFromCreditTransactions([FromBody] CreateBLFromCTsDto dto)
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

            // Validate none are already in BL or invoiced
            var alreadyProcessed = creditTransactions.Where(ct => ct.EstEnBL || ct.Facture).ToList();
            if (alreadyProcessed.Any())
            {
                return BadRequest(new CTConversionResultDto
                {
                    Success = false,
                    Message = "Certaines transactions sont déjà en BL ou facturées.",
                    Errors = alreadyProcessed.Select(ct => $"CT {ct.NumeroTransaction} déjà traité").ToList()
                });
            }

            // Create BL
            var numeroBL = await GenerateNextNumeroBLAsync();
            var bonLivraison = new BonLivraison
            {
                NumeroBL = numeroBL,
                DateBL = dto.DateBL ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ClientID = dto.ClientID > 0 ? dto.ClientID : clientIds.First(),
                Notes = dto.Notes,
                EstFacture = false,
                AjoutePar = dto.CreatedByUserId,
                DateCreation = DateTime.UtcNow
            };

            // Create BL details from CTs
            decimal montantTotal = 0;
            foreach (var ct in creditTransactions)
            {
                var detail = new BonLivraisonDetails
                {
                    ProduitID = ct.ProduitID,
                    ServiceID = ct.ServiceID,
                    Quantite = ct.Quantite,
                    PrixUnitaire = ct.PrixTTC,
                    MontantLigne = ct.MontantTotal,
                    Description = ct.Produit?.Description ?? ct.Service?.Description
                };
                bonLivraison.Details.Add(detail);
                montantTotal += ct.MontantTotal;
            }

            bonLivraison.MontantTotal = montantTotal;

            _context.BonsLivraison.Add(bonLivraison);
            await _context.SaveChangesAsync();

            // Mark CTs as in BL
            foreach (var ct in creditTransactions)
            {
                ct.EstEnBL = true;
                ct.BonLivraisonID = bonLivraison.ID;
                ct.DateModification = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new CTConversionResultDto
            {
                Success = true,
                Message = $"Bon de livraison {numeroBL} créé avec succès.",
                BonLivraisonID = bonLivraison.ID,
                NumeroBL = numeroBL,
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
                Message = "Erreur lors de la création du BL.",
                Errors = [ex.Message, ex.InnerException?.Message ?? ""]
            });
        }
    }

    // POST: api/BonLivraisons/merge (? NEW - Merge multiple BLs into one)
    [HttpPost("merge")]
    public async Task<ActionResult<MergeBLsResultDto>> MergeBLs([FromBody] MergeBLsDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate input
            if (dto.BonLivraisonIds == null || dto.BonLivraisonIds.Count < 2)
            {
                return BadRequest(new MergeBLsResultDto
                {
                    Success = false,
                    Message = "Veuillez sélectionner au moins 2 bons de livraison à fusionner.",
                    Errors = ["BonLivraisonIds doit contenir au moins 2 IDs"]
                });
            }

            // Load BLs with details
            var bonsLivraison = await _context.BonsLivraison
                .Include(bl => bl.Details)
                .Include(bl => bl.CreditTransactions)
                .Where(bl => dto.BonLivraisonIds.Contains(bl.ID))
                .ToListAsync();

            // Validate all BLs found
            if (bonsLivraison.Count != dto.BonLivraisonIds.Count)
            {
                var foundIds = bonsLivraison.Select(bl => bl.ID).ToList();
                var missingIds = dto.BonLivraisonIds.Except(foundIds).ToList();
                return BadRequest(new MergeBLsResultDto
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
                return BadRequest(new MergeBLsResultDto
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
                return BadRequest(new MergeBLsResultDto
                {
                    Success = false,
                    Message = "Certains bons de livraison sont déjà facturés.",
                    Errors = alreadyInvoiced.Select(bl => $"BL {bl.NumeroBL} déjà facturé").ToList()
                });
            }

            // Create new merged BL
            var numeroBL = await GenerateNextNumeroBLAsync();
            var newBL = new BonLivraison
            {
                NumeroBL = numeroBL,
                DateBL = dto.DateBL ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ClientID = dto.ClientID > 0 ? dto.ClientID : clientIds.First(),
                Notes = dto.Notes ?? $"BL consolidé à partir de: {string.Join(", ", bonsLivraison.Select(bl => bl.NumeroBL))}",
                EstFacture = false,
                AjoutePar = dto.CreatedByUserId,
                DateCreation = DateTime.UtcNow
            };

            // Merge all details from source BLs
            decimal montantTotal = 0;
            foreach (var sourceBL in bonsLivraison)
            {
                foreach (var detail in sourceBL.Details)
                {
                    var newDetail = new BonLivraisonDetails
                    {
                        ProduitID = detail.ProduitID,
                        ServiceID = detail.ServiceID,
                        Quantite = detail.Quantite,
                        PrixUnitaire = detail.PrixUnitaire,
                        MontantLigne = detail.MontantLigne,
                        Description = detail.Description
                    };
                    newBL.Details.Add(newDetail);
                    montantTotal += detail.MontantLigne;
                }

                // Transfer credit transactions to new BL
                foreach (var ct in sourceBL.CreditTransactions)
                {
                    ct.BonLivraisonID = null; // Will be updated after new BL is saved
                    ct.DateModification = DateTime.UtcNow;
                }
            }

            newBL.MontantTotal = montantTotal;

            _context.BonsLivraison.Add(newBL);
            await _context.SaveChangesAsync();

            // Update credit transactions to point to new BL
            foreach (var sourceBL in bonsLivraison)
            {
                foreach (var ct in sourceBL.CreditTransactions)
                {
                    ct.BonLivraisonID = newBL.ID;
                    ct.DateModification = DateTime.UtcNow;
                }
            }

            // Delete old BLs (details will cascade delete)
            foreach (var oldBL in bonsLivraison)
            {
                _context.BonLivraisonDetails.RemoveRange(oldBL.Details);
                _context.BonsLivraison.Remove(oldBL);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new MergeBLsResultDto
            {
                Success = true,
                Message = $"BL consolidé {numeroBL} créé avec succès.",
                NewBonLivraisonID = newBL.ID,
                NewNumeroBL = numeroBL,
                MontantTotal = montantTotal,
                BLsMerged = bonsLivraison.Count
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new MergeBLsResultDto
            {
                Success = false,
                Message = "Erreur lors de la fusion des BLs.",
                Errors = new List<string> { ex.Message, ex.InnerException?.Message ?? "" }
            });
        }
    }

    // PUT: api/BonLivraisons/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBonLivraison(int id, [FromBody] UpdateBonLivraisonDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch.");

        var bonLivraison = await _context.BonsLivraison
            .Include(bl => bl.Details)
            .FirstOrDefaultAsync(bl => bl.ID == id);

        if (bonLivraison == null)
            return NotFound();

        // Prevent modification of invoiced BLs
        if (bonLivraison.EstFacture)
        {
            return BadRequest("Impossible de modifier un bon de livraison déjà facturé.");
        }

        // Update header fields
        bonLivraison.NumeroBL = dto.NumeroBL;
        bonLivraison.DateBL = dto.DateBL;
        bonLivraison.ClientID = dto.ClientID;
        bonLivraison.Notes = dto.Notes;
        bonLivraison.ModifiePar = dto.ModifiePar;
        bonLivraison.DateModification = DateTime.UtcNow;

        // Remove old details and add new ones
        _context.BonLivraisonDetails.RemoveRange(bonLivraison.Details);

        decimal montantTotal = 0;
        foreach (var detailDto in dto.Details)
        {
            var detail = _mapper.Map<BonLivraisonDetails>(detailDto);
            detail.BonLivraisonID = id;
            detail.MontantLigne = detail.Quantite * detail.PrixUnitaire;
            montantTotal += detail.MontantLigne;
            _context.BonLivraisonDetails.Add(detail);
        }

        bonLivraison.MontantTotal = montantTotal;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BonLivraisonExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/BonLivraisons/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBonLivraison(int id)
    {
        var bonLivraison = await _context.BonsLivraison
            .Include(bl => bl.Details)
            .Include(bl => bl.CreditTransactions)
            .FirstOrDefaultAsync(bl => bl.ID == id);

        if (bonLivraison == null)
            return NotFound();

        // Prevent deletion of invoiced BLs
        if (bonLivraison.EstFacture)
        {
            return BadRequest("Impossible de supprimer un bon de livraison déjà facturé.");
        }

        // Unlink CreditTransactions (set EstEnBL = false, BonLivraisonID = null)
        foreach (var ct in bonLivraison.CreditTransactions)
        {
            ct.EstEnBL = false;
            ct.BonLivraisonID = null;
            ct.DateModification = DateTime.UtcNow;
        }

        // Delete cascade - remove details first
        if (bonLivraison.Details.Any())
        {
            _context.BonLivraisonDetails.RemoveRange(bonLivraison.Details);
        }

        _context.BonsLivraison.Remove(bonLivraison);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BonLivraisonExists(int id)
    {
        return _context.BonsLivraison.Any(e => e.ID == id);
    }

    private async Task<string> GenerateNextNumeroBLAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"BL-{currentYear}-";

        var lastBL = await _context.BonsLivraison
            .Where(bl => bl.NumeroBL.StartsWith(prefix))
            .OrderByDescending(bl => bl.NumeroBL)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastBL?.NumeroBL != null)
        {
            var numberPart = lastBL.NumeroBL.Replace(prefix, "");
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D5}"; // e.g., BL-2025-00001
    }
}
