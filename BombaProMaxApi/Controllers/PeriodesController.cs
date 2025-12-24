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
using BombaProMaxApi.Services;
using Microsoft.Extensions.Logging;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStockLotService _stockLotService;
        private readonly ILogger<PeriodesController> _logger;

        public PeriodesController(
            AppDbContext context, 
            IMapper mapper,
            IStockLotService stockLotService,
            ILogger<PeriodesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _stockLotService = stockLotService;
            _logger = logger;
        }

        // GET: api/Periodes
        [HttpGet]
        public async Task<ActionResult<List<PeriodeDto>>> GetPeriodes()
        {
            var periodes = await _context.Periodes
                .Include(p => p.Employe)
                .AsNoTracking()
                .OrderByDescending(p => p.DateDebut)
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDto>>(periodes);
            return Ok(dtos);
        }

        // GET: api/Periodes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PeriodeDto>> GetPeriode(int id)
        {
            var periode = await _context.Periodes
                .Include(p => p.Employe)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PeriodeID == id);

            if (periode == null)
                return NotFound();

            var dto = _mapper.Map<PeriodeDto>(periode);
            return Ok(dto);
        }

        // GET: api/Periodes/5/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<PeriodeDto>> GetPeriodeWithDetails(int id)
        {
            var periode = await _context.Periodes
                .Include(p => p.Employe)
                .Include(p => p.PeriodeDetails)
                    .ThenInclude(d => d.Pompe)
                .Include(p => p.PeriodeDetails)
                    .ThenInclude(d => d.Reservoir)
                .Include(p => p.PeriodeDetails)
                    .ThenInclude(d => d.Produit)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PeriodeID == id);

            if (periode == null)
                return NotFound();

            var dto = _mapper.Map<PeriodeDto>(periode);
            return Ok(dto);
        }

        // GET: api/Periodes/employe/5
        [HttpGet("employe/{employeId}")]
        public async Task<ActionResult<List<PeriodeDto>>> GetPeriodesByEmploye(int employeId)
        {
            var periodes = await _context.Periodes
                .Include(p => p.Employe)
                .Where(p => p.EmployeID == employeId)
                .AsNoTracking()
                .OrderByDescending(p => p.DateDebut)
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDto>>(periodes);
            return Ok(dtos);
        }

        // GET: api/Periodes/date-range?start=2025-01-01&end=2025-12-31
        [HttpGet("date-range")]
        public async Task<ActionResult<List<PeriodeDto>>> GetPeriodesByDateRange(
            [FromQuery] DateTime start, 
            [FromQuery] DateTime end)
        {
            var startUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            var periodes = await _context.Periodes
                .Include(p => p.Employe)
                .Where(p => p.DateDebut >= startUtc && p.DateFin <= endUtc)
                .AsNoTracking()
                .OrderByDescending(p => p.DateDebut)
                .ToListAsync();

            var dtos = _mapper.Map<List<PeriodeDto>>(periodes);
            return Ok(dtos);
        }

        // GET: api/Periodes/current
        [HttpGet("current")]
        public async Task<ActionResult<PeriodeDto>> GetCurrentPeriode()
        {
            var now = DateTime.UtcNow;
            var periode = await _context.Periodes
                .Include(p => p.Employe)
                .Where(p => p.DateDebut <= now && p.DateFin >= now)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (periode == null)
                return NotFound("No active period found");

            var dto = _mapper.Map<PeriodeDto>(periode);
            return Ok(dto);
        }

        // PUT: api/Periodes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPeriode(int id, [FromBody] PeriodeDto dto)
        {
            if (id != dto.PeriodeID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Periodes.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);

            // Ensure all DateTime values are in UTC for PostgreSQL
            existing.DateModification = DateTime.UtcNow;
            existing.DateDebut = DateTime.SpecifyKind(existing.DateDebut, DateTimeKind.Utc);
            existing.DateFin = DateTime.SpecifyKind(existing.DateFin, DateTimeKind.Utc);

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
                if (!PeriodeExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Periodes
        [HttpPost]
        public async Task<ActionResult<PeriodeDto>> PostPeriode([FromBody] PeriodeDto dto)
        {
            try
            {
                var entity = _mapper.Map<Periode>(dto);

                // Ensure all DateTime values are in UTC for PostgreSQL
                entity.DateCreation = DateTime.UtcNow;
                entity.DateDebut = DateTime.SpecifyKind(entity.DateDebut, DateTimeKind.Utc);
                entity.DateFin = DateTime.SpecifyKind(entity.DateFin, DateTimeKind.Utc);

                if (entity.DateModification.HasValue)
                {
                    entity.DateModification = DateTime.SpecifyKind(entity.DateModification.Value, DateTimeKind.Utc);
                }

                _context.Periodes.Add(entity);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var createdEntity = await _context.Periodes
                    .Include(p => p.Employe)
                    .FirstOrDefaultAsync(p => p.PeriodeID == entity.PeriodeID);

                var resultDto = _mapper.Map<PeriodeDto>(createdEntity);
                return CreatedAtAction(nameof(GetPeriode), new { id = entity.PeriodeID }, resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        /// <summary>
        /// Creates a Periode with all its PeriodeDetails in a single atomic transaction.
        /// This is the recommended endpoint for sales recording - triggers FIFO stock consumption.
        /// </summary>
        /// <remarks>
        /// This endpoint:
        /// 1. Creates the Periode
        /// 2. Creates all PeriodeDetails
        /// 3. Consumes stock from StockLots using FIFO
        /// 4. Updates Reservoir levels
        /// All operations are wrapped in a transaction.
        /// </remarks>
        [HttpPost("with-details")]
        public async Task<ActionResult<PeriodeWithDetailsDto>> PostPeriodeWithDetails([FromBody] PeriodeWithDetailsDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogInformation("Creating Periode with {DetailCount} details", dto.Details.Count);

                // 1. Create the Periode
                var periodeEntity = _mapper.Map<Periode>(dto.Periode);
                periodeEntity.DateCreation = DateTime.UtcNow;
                periodeEntity.DateDebut = DateTime.SpecifyKind(periodeEntity.DateDebut, DateTimeKind.Utc);
                periodeEntity.DateFin = DateTime.SpecifyKind(periodeEntity.DateFin, DateTimeKind.Utc);

                _context.Periodes.Add(periodeEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created Periode {PeriodeId}", periodeEntity.PeriodeID);

                // 2. Create all PeriodeDetails
                var detailEntities = new List<PeriodeDetails>();
                foreach (var detailDto in dto.Details)
                {
                    var detailEntity = new PeriodeDetails
                    {
                        PeriodeID = periodeEntity.PeriodeID,
                        PompeID = detailDto.PompeID,
                        ReservoirID = detailDto.ReservoirID,
                        ProduitID = detailDto.ProduitID,
                        PrixCarburant = detailDto.PrixCarburant,
                        CompteurElectroniqueDebut = detailDto.CompteurElectroniqueDebut,
                        CompteurElectroniqueFinal = detailDto.CompteurElectroniqueFinal,
                        CompteurMecaniqueDebut = detailDto.CompteurMecaniqueDebut,
                        CompteurMecaniqueFinal = detailDto.CompteurMecaniqueFinal
                    };
                    detailEntities.Add(detailEntity);
                }

                _context.PeriodeDetails.AddRange(detailEntities);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} PeriodeDetails", detailEntities.Count);

                // 3. Consume stock from StockLots (FIFO) for each detail
                foreach (var detail in detailEntities)
                {
                    if (detail.ReservoirID.HasValue && detail.ProduitID.HasValue)
                    {
                        var quantiteVendue = detail.QuantiteVendue;
                        if (quantiteVendue > 0)
                        {
                            _logger.LogInformation(
                                "Consuming {Quantite}L from Reservoir {ReservoirId} for PeriodeDetail {DetailId}",
                                quantiteVendue, detail.ReservoirID, detail.PeriodeDetailID);

                            await _stockLotService.ConsumeAsync(
                                detail.ProduitID.Value,
                                detail.ReservoirID.Value,
                                quantiteVendue,
                                detail.PeriodeDetailID);
                        }
                    }
                }

                // 4. Save all changes and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created Periode {PeriodeId} with stock consumption", periodeEntity.PeriodeID);

                // Reload with navigation properties for response
                var createdPeriode = await _context.Periodes
                    .Include(p => p.Employe)
                    .FirstOrDefaultAsync(p => p.PeriodeID == periodeEntity.PeriodeID);

                var createdDetails = await _context.PeriodeDetails
                    .Include(d => d.Pompe)
                    .Include(d => d.Reservoir)
                    .Include(d => d.Produit)
                    .Where(d => d.PeriodeID == periodeEntity.PeriodeID)
                    .ToListAsync();

                var result = new PeriodeWithDetailsDto
                {
                    Periode = _mapper.Map<PeriodeDto>(createdPeriode),
                    Details = _mapper.Map<List<PeriodeDetailsDto>>(createdDetails)
                };

                return CreatedAtAction(nameof(GetPeriode), new { id = periodeEntity.PeriodeID }, result);
            }
            catch (InvalidOperationException ex)
            {
                // Stock insuffisant - rollback and return error
                await transaction.RollbackAsync();
                _logger.LogWarning("Stock consumption failed: {Message}", ex.Message);
                return BadRequest(new { error = "Stock insuffisant", message = ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating Periode with details");
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        // DELETE: api/Periodes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeriode(int id)
        {
            var periode = await _context.Periodes
                .Include(p => p.PeriodeDetails)
                .FirstOrDefaultAsync(p => p.PeriodeID == id);

            if (periode == null)
                return NotFound();

            // Delete cascade - remove details first
            if (periode.PeriodeDetails.Any())
            {
                _context.PeriodeDetails.RemoveRange(periode.PeriodeDetails);
            }

            _context.Periodes.Remove(periode);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PeriodeExists(int id)
        {
            return _context.Periodes.Any(e => e.PeriodeID == id);
        }
    }
}
