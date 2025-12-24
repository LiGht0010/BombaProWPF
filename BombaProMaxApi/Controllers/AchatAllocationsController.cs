using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using BombaProMaxApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchatAllocationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStockLotService _stockLotService;
        private readonly ILogger<AchatAllocationsController> _logger;

        public AchatAllocationsController(
            AppDbContext context, 
            IMapper mapper,
            IStockLotService stockLotService,
            ILogger<AchatAllocationsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _stockLotService = stockLotService;
            _logger = logger;
        }

        // GET: api/AchatAllocations
        [HttpGet]
        public async Task<ActionResult<List<AchatAllocationDto>>> GetAchatAllocations()
        {
            var allocations = await _context.AchatAllocations
                .Include(a => a.Achat)
                .Include(a => a.Reservoir)
                    .ThenInclude(r => r.Produit)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatAllocationDto>>(allocations);
            return Ok(dtos);
        }

        // GET: api/AchatAllocations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AchatAllocationDto>> GetAchatAllocation(int id)
        {
            var allocation = await _context.AchatAllocations
                .Include(a => a.Achat)
                .Include(a => a.Reservoir)
                    .ThenInclude(r => r.Produit)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == id);

            if (allocation == null)
                return NotFound();

            var dto = _mapper.Map<AchatAllocationDto>(allocation);
            return Ok(dto);
        }

        // GET: api/AchatAllocations/achat/5
        [HttpGet("achat/{achatId}")]
        public async Task<ActionResult<List<AchatAllocationDto>>> GetAllocationsByAchat(int achatId)
        {
            var allocations = await _context.AchatAllocations
                .Include(a => a.Achat)
                .Include(a => a.Reservoir)
                    .ThenInclude(r => r.Produit)
                .Where(a => a.AchatID == achatId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatAllocationDto>>(allocations);
            return Ok(dtos);
        }

        // GET: api/AchatAllocations/reservoir/5
        [HttpGet("reservoir/{reservoirId}")]
        public async Task<ActionResult<List<AchatAllocationDto>>> GetAllocationsByReservoir(int reservoirId)
        {
            var allocations = await _context.AchatAllocations
                .Include(a => a.Achat)
                .Include(a => a.Reservoir)
                    .ThenInclude(r => r.Produit)
                .Where(a => a.ReservoirID == reservoirId)
                .OrderByDescending(a => a.DateAllocation)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatAllocationDto>>(allocations);
            return Ok(dtos);
        }

        // GET: api/AchatAllocations/available-reservoirs/5
        /// <summary>
        /// Gets reservoirs available for allocation for a specific product (fuel type).
        /// Returns reservoirs that either have the same product or are empty.
        /// </summary>
        [HttpGet("available-reservoirs/{produitId}")]
        public async Task<ActionResult<List<ReservoirAllocationInfoDto>>> GetAvailableReservoirs(int produitId)
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Where(r => r.ProduitID == produitId || r.ProduitID == null || r.NiveauDeCarburant == 0)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirAllocationInfoDto>>(reservoirs);

            // Mark compatibility
            foreach (var dto in dtos)
            {
                dto.EstCompatible = dto.ProduitID == produitId || dto.EstVide;
            }

            return Ok(dtos.OrderByDescending(r => r.EstCompatible).ThenBy(r => r.Numero).ToList());
        }

        // GET: api/AchatAllocations/check-achat-allocated/5
        /// <summary>
        /// Checks if an achat has already been fully allocated
        /// </summary>
        [HttpGet("check-achat-allocated/{achatId}")]
        public async Task<ActionResult<object>> CheckAchatAllocated(int achatId)
        {
            var achat = await _context.Achats
                .Include(a => a.Produit)
                    .ThenInclude(p => p!.Categorie)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == achatId);

            if (achat == null)
                return NotFound("Achat not found");

            var totalAllocated = await _context.AchatAllocations
                .Where(a => a.AchatID == achatId && a.Statut != "Annulée")
                .SumAsync(a => a.QuantiteAllouee);

            var isFuel = achat.Produit?.Categorie?.Nom?.ToLower() == "carburant" ||
                         achat.Produit?.Categorie?.Nom?.ToLower() == "carburants";

            return Ok(new
            {
                AchatID = achatId,
                Quantite = achat.Quantite ?? 0,
                TotalAlloue = totalAllocated,
                Restant = (achat.Quantite ?? 0) - totalAllocated,
                EstCompletementAlloue = totalAllocated >= (achat.Quantite ?? 0),
                EstCarburant = isFuel,
                ProduitID = achat.ProduitID,
                ProduitNom = achat.Produit?.Description
            });
        }

        // POST: api/AchatAllocations
        [HttpPost]
        public async Task<ActionResult<AchatAllocationDto>> PostAchatAllocation([FromBody] AchatAllocationDto dto)
        {
            // Validate reservoir exists and has capacity
            var reservoir = await _context.Reservoirs.FindAsync(dto.ReservoirID);
            if (reservoir == null)
                return BadRequest("Reservoir not found");

            var espaceDisponible = reservoir.Capacite - reservoir.NiveauDeCarburant;
            if (dto.QuantiteAllouee > espaceDisponible)
                return BadRequest($"Quantité dépasse l'espace disponible ({espaceDisponible:N2} L)");

            // Get the Achat to retrieve PrixAchatUnitaire and ProduitID
            var achat = await _context.Achats
                .Include(a => a.Produit)
                    .ThenInclude(p => p!.Categorie)
                .FirstOrDefaultAsync(a => a.ID == dto.AchatID);

            if (achat == null)
                return BadRequest("Achat not found");

            var entity = _mapper.Map<AchatAllocation>(dto);
            entity.DateAllocation = DateTime.UtcNow;
            entity.Statut = "Confirmée";

            _context.AchatAllocations.Add(entity);

            // Update reservoir fuel level (handled by StockLotService.CreateStockLotAsync now)
            // We still update it here for backward compatibility, but StockLotService also updates it
            // Comment out to avoid double-update:
            // reservoir.NiveauDeCarburant += dto.QuantiteAllouee;
            reservoir.DateModification = DateTime.UtcNow;

            // If reservoir was empty, assign the product type
            if (!reservoir.ProduitID.HasValue && achat.ProduitID.HasValue)
            {
                reservoir.ProduitID = achat.ProduitID;
            }

            await _context.SaveChangesAsync();

            // Create StockLot for FIFO tracking (only for fuel products)
            var categoryName = achat.Produit?.Categorie?.Nom?.ToLower();
            if (categoryName == "carburant" || categoryName == "carburants")
            {
                var prixAchat = achat.PrixAchatUnitaire ?? 0;
                if (prixAchat == 0 && achat.Quantite > 0 && achat.Cout.HasValue)
                {
                    // Calculate unit price from total cost if not set
                    prixAchat = achat.Cout.Value / achat.Quantite.Value;
                }

                await _stockLotService.CreateStockLotAsync(
                    achatId: dto.AchatID,
                    reservoirId: dto.ReservoirID,
                    produitId: achat.ProduitID ?? reservoir.ProduitID ?? 0,
                    quantite: dto.QuantiteAllouee,
                    prixAchat: prixAchat);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Created StockLot for Allocation: Achat {AchatId}, Reservoir {ReservoirId}, Qty {Qty}L",
                    dto.AchatID, dto.ReservoirID, dto.QuantiteAllouee);
            }

            // Reload with related entities
            await _context.Entry(entity).Reference(e => e.Achat).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Reservoir).LoadAsync();
            if (entity.Reservoir != null)
            {
                await _context.Entry(entity.Reservoir).Reference(r => r.Produit).LoadAsync();
            }

            var resultDto = _mapper.Map<AchatAllocationDto>(entity);
            return CreatedAtAction(nameof(GetAchatAllocation), new { id = entity.ID }, resultDto);
        }

        // POST: api/AchatAllocations/batch
        /// <summary>
        /// Allocates fuel from an achat to multiple reservoirs in a single transaction.
        /// Updates reservoir fuel levels and creates allocation records.
        /// </summary>
        [HttpPost("batch")]
        public async Task<ActionResult<BatchAllocationResponseDto>> PostBatchAllocation([FromBody] BatchAllocationRequestDto request)
        {
            // Validate achat exists
            var achat = await _context.Achats
                .Include(a => a.Produit)
                    .ThenInclude(p => p!.Categorie)
                .FirstOrDefaultAsync(a => a.ID == request.AchatID);

            if (achat == null)
                return NotFound(new BatchAllocationResponseDto
                {
                    Success = false,
                    Message = "Achat non trouvé"
                });

            // Validate it's a fuel product
            var categoryName = achat.Produit?.Categorie?.Nom?.ToLower();
            if (categoryName != "carburant" && categoryName != "carburants")
            {
                return BadRequest(new BatchAllocationResponseDto
                {
                    Success = false,
                    Message = "L'allocation n'est applicable qu'aux produits de type carburant"
                });
            }

            // Calculate total being allocated
            var totalAllocating = request.Allocations.Sum(a => a.QuantiteAllouee);

            // Check existing allocations
            var existingAllocated = await _context.AchatAllocations
                .Where(a => a.AchatID == request.AchatID && a.Statut != "Annulée")
                .SumAsync(a => a.QuantiteAllouee);

            var quantiteAchat = achat.Quantite ?? 0;
            var quantiteRestante = quantiteAchat - existingAllocated;

            if (totalAllocating > quantiteRestante)
            {
                return BadRequest(new BatchAllocationResponseDto
                {
                    Success = false,
                    Message = $"Quantité totale ({totalAllocating:N2} L) dépasse la quantité restante ({quantiteRestante:N2} L)"
                });
            }

            // Validate each reservoir
            var reservoirIds = request.Allocations.Select(a => a.ReservoirID).ToList();
            var reservoirs = await _context.Reservoirs
                .Where(r => reservoirIds.Contains(r.ID))
                .ToDictionaryAsync(r => r.ID);

            foreach (var allocation in request.Allocations)
            {
                if (!reservoirs.TryGetValue(allocation.ReservoirID, out var reservoir))
                {
                    return BadRequest(new BatchAllocationResponseDto
                    {
                        Success = false,
                        Message = $"Réservoir {allocation.ReservoirID} non trouvé"
                    });
                }

                var espaceDisponible = reservoir.Capacite - reservoir.NiveauDeCarburant;
                if (allocation.QuantiteAllouee > espaceDisponible)
                {
                    return BadRequest(new BatchAllocationResponseDto
                    {
                        Success = false,
                        Message = $"Réservoir {reservoir.Numero}: quantité ({allocation.QuantiteAllouee:N2} L) dépasse l'espace disponible ({espaceDisponible:N2} L)"
                    });
                }

                // Validate product compatibility
                if (reservoir.ProduitID.HasValue && 
                    reservoir.ProduitID != achat.ProduitID && 
                    reservoir.NiveauDeCarburant > 0)
                {
                    return BadRequest(new BatchAllocationResponseDto
                    {
                        Success = false,
                        Message = $"Réservoir {reservoir.Numero} contient un carburant différent"
                    });
                }
            }

            // All validations passed - create allocations and update reservoirs
            var createdAllocations = new List<AchatAllocation>();

            // Get purchase price for StockLot creation
            var prixAchat = achat.PrixAchatUnitaire ?? 0;
            if (prixAchat == 0 && achat.Quantite > 0 && achat.Cout.HasValue)
            {
                prixAchat = achat.Cout.Value / achat.Quantite.Value;
            }

            foreach (var allocationItem in request.Allocations)
            {
                if (allocationItem.QuantiteAllouee <= 0)
                    continue;

                var reservoir = reservoirs[allocationItem.ReservoirID];

                // Create allocation record
                var allocation = new AchatAllocation
                {
                    AchatID = request.AchatID,
                    ReservoirID = allocationItem.ReservoirID,
                    QuantiteAllouee = allocationItem.QuantiteAllouee,
                    DateAllocation = DateTime.UtcNow,
                    Notes = allocationItem.Notes ?? request.Notes,
                    Statut = "Confirmée",
                    UtilisateurAllocation = request.UtilisateurAllocation
                };

                _context.AchatAllocations.Add(allocation);
                createdAllocations.Add(allocation);

                // Update reservoir fuel level - now handled by StockLotService
                // reservoir.NiveauDeCarburant += allocationItem.QuantiteAllouee;
                reservoir.DateModification = DateTime.UtcNow;

                // If reservoir was empty, assign the product type
                if (!reservoir.ProduitID.HasValue && achat.ProduitID.HasValue)
                {
                    reservoir.ProduitID = achat.ProduitID;
                }

                // Create StockLot for FIFO tracking
                await _stockLotService.CreateStockLotAsync(
                    achatId: request.AchatID,
                    reservoirId: allocationItem.ReservoirID,
                    produitId: achat.ProduitID ?? reservoir.ProduitID ?? 0,
                    quantite: allocationItem.QuantiteAllouee,
                    prixAchat: prixAchat);

                _logger.LogInformation(
                    "Created StockLot for Batch Allocation: Achat {AchatId}, Reservoir {ReservoirId}, Qty {Qty}L",
                    request.AchatID, allocationItem.ReservoirID, allocationItem.QuantiteAllouee);
            }

            await _context.SaveChangesAsync();

            // Load related entities for response
            foreach (var allocation in createdAllocations)
            {
                await _context.Entry(allocation).Reference(a => a.Achat).LoadAsync();
                await _context.Entry(allocation).Reference(a => a.Reservoir).LoadAsync();
                if (allocation.Reservoir != null)
                {
                    await _context.Entry(allocation.Reservoir).Reference(r => r.Produit).LoadAsync();
                }
            }

            var allocationDtos = _mapper.Map<List<AchatAllocationDto>>(createdAllocations);

            return Ok(new BatchAllocationResponseDto
            {
                Success = true,
                Message = $"Allocation réussie: {totalAllocating:N2} L répartis sur {createdAllocations.Count} réservoir(s)",
                AchatID = request.AchatID,
                TotalAlloue = totalAllocating,
                Allocations = allocationDtos
            });
        }

        // PUT: api/AchatAllocations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAchatAllocation(int id, [FromBody] AchatAllocationDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch");

            var existing = await _context.AchatAllocations
                .Include(a => a.Reservoir)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (existing == null)
                return NotFound();

            // If quantity changed, update reservoir level
            if (existing.QuantiteAllouee != dto.QuantiteAllouee && existing.Reservoir != null)
            {
                var difference = dto.QuantiteAllouee - existing.QuantiteAllouee;
                var newLevel = existing.Reservoir.NiveauDeCarburant + difference;

                if (newLevel < 0)
                    return BadRequest("La modification rendrait le niveau du réservoir négatif");

                if (newLevel > existing.Reservoir.Capacite)
                    return BadRequest("La modification dépasserait la capacité du réservoir");

                existing.Reservoir.NiveauDeCarburant = newLevel;
                existing.Reservoir.DateModification = DateTime.UtcNow;
            }

            existing.QuantiteAllouee = dto.QuantiteAllouee;
            existing.Notes = dto.Notes;
            existing.Statut = dto.Statut;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/AchatAllocations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchatAllocation(int id)
        {
            var allocation = await _context.AchatAllocations
                .Include(a => a.Reservoir)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (allocation == null)
                return NotFound();

            // Reverse the fuel level in reservoir
            if (allocation.Reservoir != null && allocation.Statut == "Confirmée")
            {
                allocation.Reservoir.NiveauDeCarburant = Math.Max(0, 
                    allocation.Reservoir.NiveauDeCarburant - allocation.QuantiteAllouee);
                allocation.Reservoir.DateModification = DateTime.UtcNow;
            }

            _context.AchatAllocations.Remove(allocation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/AchatAllocations/cancel/5
        /// <summary>
        /// Cancels an allocation and reverses the reservoir fuel level.
        /// Also reverses the associated StockLot if it hasn't been consumed.
        /// </summary>
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelAllocation(int id)
        {
            var allocation = await _context.AchatAllocations
                .Include(a => a.Reservoir)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (allocation == null)
                return NotFound();

            if (allocation.Statut == "Annulée")
                return BadRequest("L'allocation est déjà annulée");

            // Try to reverse the StockLot first
            var stockLotReversed = await _stockLotService.ReverseStockLotAsync(
                allocation.AchatID,
                allocation.ReservoirID,
                allocation.QuantiteAllouee);

            if (!stockLotReversed)
            {
                // StockLot has been consumed - cannot cancel allocation
                _logger.LogWarning(
                    "Cannot cancel allocation {AllocationId} - StockLot has been consumed",
                    id);
                return BadRequest(new 
                { 
                    Error = "Annulation impossible",
                    Message = "Le stock de cette allocation a déjà été consommé (vendu). L'annulation n'est plus possible."
                });
            }

            // StockLot was reversed successfully, now mark allocation as cancelled
            // Note: Reservoir level was already updated by ReverseStockLotAsync
            allocation.Statut = "Annulée";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled allocation {AllocationId} and reversed StockLot", id);
            return Ok(new { Message = "Allocation annulée avec succès" });
        }
    }
}
