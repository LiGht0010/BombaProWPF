using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FactureBonLivraisonController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<FactureBonLivraisonController> _logger;

    public FactureBonLivraisonController(AppDbContext context, IMapper mapper, ILogger<FactureBonLivraisonController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/FactureBonLivraison
    [HttpGet]
    public async Task<ActionResult<List<FactureBonLivraisonDto>>> GetFactureBonLivraisons()
    {
        var links = await _context.FactureBonLivraisons
            .Include(fbl => fbl.Facture)
            .Include(fbl => fbl.BonLivraison)
            .AsNoTracking()
            .OrderByDescending(fbl => fbl.DateAssociation)
            .ToListAsync();

        var dtos = _mapper.Map<List<FactureBonLivraisonDto>>(links);
        return Ok(dtos);
    }

    // GET: api/FactureBonLivraison/5
    [HttpGet("{id}")]
    public async Task<ActionResult<FactureBonLivraisonDto>> GetFactureBonLivraison(int id)
    {
        var link = await _context.FactureBonLivraisons
            .Include(fbl => fbl.Facture)
            .Include(fbl => fbl.BonLivraison)
            .AsNoTracking()
            .FirstOrDefaultAsync(fbl => fbl.ID == id);

        if (link == null)
            return NotFound();

        var dto = _mapper.Map<FactureBonLivraisonDto>(link);
        return Ok(dto);
    }

    // GET: api/FactureBonLivraison/facture/5
    [HttpGet("facture/{factureId}")]
    public async Task<ActionResult<List<FactureBonLivraisonDto>>> GetBonLivraisonsByFacture(int factureId)
    {
        var links = await _context.FactureBonLivraisons
            .Include(fbl => fbl.Facture)
            .Include(fbl => fbl.BonLivraison)
            .Where(fbl => fbl.FactureID == factureId)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<FactureBonLivraisonDto>>(links);
        return Ok(dtos);
    }

    // GET: api/FactureBonLivraison/facture/5/bls
    [HttpGet("facture/{factureId}/bls")]
    public async Task<ActionResult<List<BonLivraisonDto>>> GetFullBonLivraisonsByFacture(int factureId)
    {
        var blIds = await _context.FactureBonLivraisons
            .Where(fbl => fbl.FactureID == factureId)
            .Select(fbl => fbl.BonLivraisonID)
            .ToListAsync();

        var bonsLivraison = await _context.BonsLivraison
            .Include(bl => bl.Client)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Produit)
            .Include(bl => bl.Details)
                .ThenInclude(d => d.Service)
            .Where(bl => blIds.Contains(bl.ID))
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<BonLivraisonDto>>(bonsLivraison);
        return Ok(dtos);
    }

    // GET: api/FactureBonLivraison/bonlivraison/5
    [HttpGet("bonlivraison/{bonLivraisonId}")]
    public async Task<ActionResult<FactureBonLivraisonDto>> GetFactureByBonLivraison(int bonLivraisonId)
    {
        var link = await _context.FactureBonLivraisons
            .Include(fbl => fbl.Facture)
            .Include(fbl => fbl.BonLivraison)
            .AsNoTracking()
            .FirstOrDefaultAsync(fbl => fbl.BonLivraisonID == bonLivraisonId);

        if (link == null)
            return NotFound();

        var dto = _mapper.Map<FactureBonLivraisonDto>(link);
        return Ok(dto);
    }

    // GET: api/FactureBonLivraison/next-numero-facture
    [HttpGet("next-numero-facture")]
    public async Task<ActionResult<string>> GetNextNumeroFacture()
    {
        var numero = await GenerateNextNumeroFactureAsync();
        return Ok(numero);
    }

    // POST: api/FactureBonLivraison/from-bls
    [HttpPost("from-bls")]
    public async Task<ActionResult<FacturationResultDto>> CreateFactureFromBLs([FromBody] CreateFactureFromBLsDto request)
    {
        if (request.BonLivraisonIds == null || !request.BonLivraisonIds.Any())
        {
            return BadRequest(new FacturationResultDto
            {
                Success = false,
                Message = "Aucun bon de livraison sélectionné.",
                Errors = new List<string> { "La liste des BLs ne peut pas être vide." }
            });
        }

        // Use execution strategy to support retry on failure with user-initiated transactions
        var strategy = _context.Database.CreateExecutionStrategy();

        FacturationResultDto? result = null;

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Load all selected BLs with their details where EstFacture == false
                    var bonsLivraison = await _context.BonsLivraison
                        .Include(bl => bl.Details)
                            .ThenInclude(d => d.Produit)
                        .Include(bl => bl.Details)
                            .ThenInclude(d => d.Service)
                        .Include(bl => bl.Client)
                        .Where(bl => request.BonLivraisonIds.Contains(bl.ID) && !bl.EstFacture)
                        .ToListAsync();

                    // Validate all requested BLs were found and are not already invoiced
                    if (bonsLivraison.Count != request.BonLivraisonIds.Count)
                    {
                        var foundIds = bonsLivraison.Select(bl => bl.ID).ToHashSet();
                        var missingOrInvoiced = request.BonLivraisonIds.Where(id => !foundIds.Contains(id)).ToList();

                        result = new FacturationResultDto
                        {
                            Success = false,
                            Message = "Certains BLs n'existent pas ou sont déjà facturés.",
                            Errors = new List<string> { $"BLs non trouvés ou déjà facturés: {string.Join(", ", missingOrInvoiced)}" }
                        };
                        return;
                    }

                    // 2. Validate all BLs belong to the same client
                    var clientIds = bonsLivraison.Select(bl => bl.ClientID).Distinct().ToList();
                    if (clientIds.Count > 1)
                    {
                        result = new FacturationResultDto
                        {
                            Success = false,
                            Message = "Tous les BLs doivent appartenir au même client.",
                            Errors = new List<string> { "Les BLs sélectionnés appartiennent à plusieurs clients différents." }
                        };
                        return;
                    }

                    var clientId = clientIds.First();

                    // 3. Generate invoice number
                    var numeroFacture = await GenerateNextNumeroFactureAsync();

                    // 4. Create the Facture
                    var facture = new Facture
                    {
                        NumeroFacture = numeroFacture,
                        DateFacture = request.DateFacture ?? DateOnly.FromDateTime(DateTime.UtcNow),
                        ClientID = clientId,
                        MontantTotal = 0, // Will be calculated
                        Statut = "Non Payée"
                    };

                    _context.Factures.Add(facture);
                    await _context.SaveChangesAsync(); // Save to get the Facture ID

                    // 5. Process each BL
                    decimal montantTotal = 0;

                    foreach (var bl in bonsLivraison)
                    {
                        // Create junction record
                        var factureBL = new FactureBonLivraison
                        {
                            FactureID = facture.ID,
                            BonLivraisonID = bl.ID,
                            DateAssociation = DateTime.UtcNow
                        };
                        _context.FactureBonLivraisons.Add(factureBL);

                        // Copy each BL detail to ElementsFacture
                        foreach (var detail in bl.Details)
                        {
                            var elementFacture = new ElementsFacture
                            {
                                FactureID = facture.ID,
                                ProduitID = detail.ProduitID,
                                ServiceID = detail.ServiceID,
                                Quantite = detail.Quantite,
                                PrixUnitaire = detail.PrixUnitaire
                            };
                            _context.ElementsFactures.Add(elementFacture);
                        }

                        // Mark BL as invoiced
                        bl.EstFacture = true;
                        bl.DateModification = DateTime.UtcNow;

                        // Add to total
                        montantTotal += bl.MontantTotal;
                    }

                    // 6. Update Facture total
                    facture.MontantTotal = montantTotal;

                    // 7. Save all changes
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Facture {NumeroFacture} created from {BLCount} BLs for client {ClientId}. Total: {MontantTotal}",
                        numeroFacture, bonsLivraison.Count, clientId, montantTotal);

                    result = new FacturationResultDto
                    {
                        Success = true,
                        Message = $"Facture {numeroFacture} créée avec succès.",
                        FactureID = facture.ID,
                        NumeroFacture = numeroFacture,
                        MontantTotal = montantTotal,
                        BLsFactures = bonsLivraison.Count
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating facture from BLs: {BLIds}", string.Join(", ", request.BonLivraisonIds));

            return StatusCode(500, new FacturationResultDto
            {
                Success = false,
                Message = "Une erreur est survenue lors de la création de la facture.",
                Errors = new List<string> { ex.Message }
            });
        }

        if (result == null)
        {
            return StatusCode(500, new FacturationResultDto { Success = false, Message = "Erreur inattendue." });
        }

        return result.Success ? Ok(result) : BadRequest(result);
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
