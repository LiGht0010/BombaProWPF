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
    public class ReservoirsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStockLotService _stockLotService;
        private readonly ILogger<ReservoirsController> _logger;

        public ReservoirsController(
            AppDbContext context, 
            IMapper mapper,
            IStockLotService stockLotService,
            ILogger<ReservoirsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _stockLotService = stockLotService;
            _logger = logger;
        }

        // GET: api/Reservoirs
        [HttpGet]
        public async Task<ActionResult<List<ReservoirDto>>> GetReservoirs()
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.Calibrations)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirDto>>(reservoirs);
            
            // Enhance DTOs with stock status
            foreach (var dto in dtos)
            {
                dto.HasStockLots = await _stockLotService.HasAnyStockLotsAsync(dto.ID);
            }
            
            return Ok(dtos);
        }

        // GET: api/Reservoirs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservoirDto>> GetReservoir(int id)
        {
            var reservoir = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.Calibrations)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservoir == null)
                return NotFound();

            var dto = _mapper.Map<ReservoirDto>(reservoir);
            dto.HasStockLots = await _stockLotService.HasAnyStockLotsAsync(id);
            
            return Ok(dto);
        }

        // GET: api/Reservoirs/5/stock-status
        /// <summary>
        /// Gets comprehensive stock status for a reservoir.
        /// Includes opening balance eligibility and current stock level.
        /// </summary>
        [HttpGet("{id}/stock-status")]
        public async Task<ActionResult<ReservoirStockStatusDto>> GetStockStatus(int id)
        {
            var reservoir = await _context.Reservoirs
                .Include(r => r.Produit)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservoir == null)
                return NotFound();

            var hasStockLots = await _stockLotService.HasAnyStockLotsAsync(id);
            var hasOpeningBalance = await _stockLotService.HasOpeningBalanceAsync(id);

            return Ok(new ReservoirStockStatusDto
            {
                ReservoirID = reservoir.ID,
                ReservoirNumero = reservoir.Numero,
                ProduitID = reservoir.ProduitID,
                ProduitNom = reservoir.Produit?.Description,
                Capacite = reservoir.Capacite,
                NiveauActuel = reservoir.NiveauDeCarburant,
                HasStockLots = hasStockLots,
                HasOpeningBalance = hasOpeningBalance,
                BlockingReason = hasStockLots 
                    ? "Le réservoir possède déjà des lots de stock. Le stock initial ne peut être créé que pour un réservoir vide."
                    : null
            });
        }

        // GET: api/Reservoirs/needing-opening-balance
        /// <summary>
        /// Gets all reservoirs that need opening balance setup.
        /// These are reservoirs with no stock lots that may need initial inventory.
        /// </summary>
        [HttpGet("needing-opening-balance")]
        public async Task<ActionResult<List<ReservoirStockStatusDto>>> GetReservoirsNeedingOpeningBalance()
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.StockLots)
                .Where(r => !r.StockLots.Any(sl => sl.Statut != "Annulé"))
                .AsNoTracking()
                .ToListAsync();

            var result = reservoirs.Select(r => new ReservoirStockStatusDto
            {
                ReservoirID = r.ID,
                ReservoirNumero = r.Numero,
                ProduitID = r.ProduitID,
                ProduitNom = r.Produit?.Description,
                Capacite = r.Capacite,
                NiveauActuel = r.NiveauDeCarburant,
                HasStockLots = false,
                HasOpeningBalance = false,
                BlockingReason = null
            }).ToList();

            return Ok(result);
        }

        // POST: api/Reservoirs/5/opening-balance
        /// <summary>
        /// Creates an opening balance for a reservoir directly from the reservoir endpoint.
        /// Convenience wrapper around StockLotsController.CreateOpeningBalance.
        /// </summary>
        [HttpPost("{id}/opening-balance")]
        public async Task<ActionResult<OpeningBalanceResultDto>> CreateOpeningBalance(
            int id, 
            [FromBody] OpeningBalanceCreateDto dto)
        {
            // Ensure the DTO matches the route ID
            if (dto.ReservoirID != 0 && dto.ReservoirID != id)
            {
                return BadRequest("ReservoirID in body doesn't match route ID");
            }
            dto.ReservoirID = id;

            var reservoir = await _context.Reservoirs
                .Include(r => r.Produit)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservoir == null)
                return NotFound($"Reservoir {id} not found");

            var produit = await _context.Produits.FindAsync(dto.ProduitID);
            if (produit == null)
                return BadRequest($"Produit {dto.ProduitID} not found");

            try
            {
                _logger.LogInformation(
                    "Creating opening balance via Reservoir endpoint: Reservoir {ReservoirId}, {Quantite}L",
                    id, dto.Quantite);

                var stockLotId = await _stockLotService.CreateOpeningBalanceAsync(
                    dto.ReservoirID,
                    dto.ProduitID,
                    dto.Quantite,
                    dto.PrixAchat,
                    dto.DateEntree,
                    dto.Notes);

                // Reload reservoir to get updated level
                await _context.Entry(reservoir).ReloadAsync();

                var result = new OpeningBalanceResultDto
                {
                    StockLotID = stockLotId,
                    ReservoirID = id,
                    ReservoirNumero = reservoir.Numero,
                    ProduitID = dto.ProduitID,
                    ProduitNom = produit.Description,
                    Quantite = dto.Quantite,
                    PrixAchat = dto.PrixAchat,
                    DateEntree = dto.DateEntree ?? DateTime.UtcNow,
                    Notes = dto.Notes,
                    NouveauNiveau = reservoir.NiveauDeCarburant,
                    Message = $"Stock initial de {dto.Quantite:N2}L créé pour le réservoir {reservoir.Numero}"
                };

                return CreatedAtAction(nameof(GetReservoir), new { id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create opening balance: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Reservoirs/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<ReservoirDto>>> GetReservoirsByProduct(int productId)
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Include(r => r.Calibrations)
                .Where(r => r.ProduitID == productId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirDto>>(reservoirs);
            return Ok(dtos);
        }

        // GET: api/Reservoirs/lowfuel?threshold=20
        [HttpGet("lowfuel")]
        public async Task<ActionResult<List<ReservoirDto>>> GetLowFuelReservoirs([FromQuery] decimal threshold = 20)
        {
            var reservoirs = await _context.Reservoirs
                .Include(r => r.Produit)
                .Where(r => r.Capacite > 0 && (r.NiveauDeCarburant / r.Capacite * 100) < threshold)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ReservoirDto>>(reservoirs);
            return Ok(dtos);
        }

        // GET: api/Reservoirs/check-numero?numero=xxx&excludeId=1
        [HttpGet("check-numero")]
        public async Task<ActionResult<bool>> CheckNumeroExists([FromQuery] string numero, [FromQuery] int? excludeId = null)
        {
            var exists = await _context.Reservoirs
                .AnyAsync(r => r.Numero.ToLower() == numero.ToLower() && r.ID != excludeId);
            return Ok(exists);
        }

        // POST: api/Reservoirs
        [HttpPost]
        public async Task<ActionResult<ReservoirDto>> CreateReservoir([FromBody] ReservoirDto dto)
        {
            var entity = _mapper.Map<Reservoir>(dto);
            entity.DateCreation = DateTime.UtcNow;
            
            // Note: NiveauDeCarburant should start at 0 for new reservoirs
            // Opening balance should be used to set initial stock level
            entity.NiveauDeCarburant = 0;

            _context.Reservoirs.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with Produit for response
            if (entity.ProduitID.HasValue)
            {
                await _context.Entry(entity).Reference(r => r.Produit).LoadAsync();
            }

            var resultDto = _mapper.Map<ReservoirDto>(entity);
            resultDto.HasStockLots = false; // New reservoir has no stock lots
            
            _logger.LogInformation(
                "Created Reservoir {ReservoirId} ({Numero}). Use opening-balance endpoint to set initial stock.",
                entity.ID, entity.Numero);
            
            return CreatedAtAction(nameof(GetReservoir), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Reservoirs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservoir(int id, [FromBody] ReservoirDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Reservoirs
                .Include(r => r.StockLots)
                .FirstOrDefaultAsync(r => r.ID == id);
                
            if (existing == null)
                return NotFound();

            // If reservoir has stock lots, prevent direct NiveauDeCarburant changes
            // Level should only change through stock operations
            var hasStockLots = existing.StockLots.Any(sl => sl.Statut != "Annulé");
            if (hasStockLots)
            {
                // Preserve the calculated level from stock lots
                var currentLevel = existing.NiveauDeCarburant;
                _mapper.Map(dto, existing);
                existing.NiveauDeCarburant = currentLevel; // Restore the level
                
                _logger.LogDebug(
                    "Reservoir {ReservoirId} has stock lots - NiveauDeCarburant preserved at {Level}L",
                    id, currentLevel);
            }
            else
            {
                _mapper.Map(dto, existing);
            }

            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Reservoirs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservoir(int id)
        {
            var reservoir = await _context.Reservoirs
                .Include(r => r.StockLots)
                .FirstOrDefaultAsync(r => r.ID == id);
                
            if (reservoir == null)
                return NotFound();

            // Check if reservoir has any stock lots
            if (reservoir.StockLots.Any())
            {
                return BadRequest(
                    "Impossible de supprimer un réservoir avec des lots de stock. " +
                    "Veuillez d'abord transférer ou consommer le stock.");
            }

            _context.Reservoirs.Remove(reservoir);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
