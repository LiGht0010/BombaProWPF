using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JaugeagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public JaugeagesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Jaugeages
    [HttpGet]
    public async Task<ActionResult<List<JaugeageDto>>> GetJaugeages()
    {
        var jaugeages = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Include(j => j.JaugeageDetails)
                .ThenInclude(d => d.Reservoir)
            .OrderByDescending(j => j.DateJaugeage)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<JaugeageDto>>(jaugeages));
    }

    // GET: api/Jaugeages/5
    [HttpGet("{id}")]
    public async Task<ActionResult<JaugeageDto>> GetJaugeage(int id)
    {
        var jaugeage = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Include(j => j.JaugeageDetails)
                .ThenInclude(d => d.Reservoir)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.ID == id);

        if (jaugeage == null)
            return NotFound();

        return Ok(_mapper.Map<JaugeageDto>(jaugeage));
    }

    // GET: api/Jaugeages/5/with-details
    /// <summary>
    /// Gets a Jaugeage with all its details included
    /// </summary>
    [HttpGet("{id}/with-details")]
    public async Task<ActionResult<JaugeageWithDetailsDto>> GetJaugeageWithDetails(int id)
    {
        var jaugeage = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Include(j => j.JaugeageDetails)
                .ThenInclude(d => d.Reservoir)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.ID == id);

        if (jaugeage == null)
            return NotFound();

        var dto = new JaugeageWithDetailsDto
        {
            ID = jaugeage.ID,
            DateJaugeage = jaugeage.DateJaugeage,
            TemoinID = jaugeage.TemoinID,
            NumeroJaugeage = jaugeage.NumeroJaugeage,
            Observations = jaugeage.Observations,
            TemoinNom = jaugeage.Temoin?.Nom,
            AjoutePar = jaugeage.AjoutePar,
            DateCreation = jaugeage.DateCreation,
            ModifiePar = jaugeage.ModifiePar,
            DateModification = jaugeage.DateModification,
            Details = _mapper.Map<List<JaugeageDetailDto>>(jaugeage.JaugeageDetails)
        };

        return Ok(dto);
    }

    // GET: api/Jaugeages/by-date?date=2024-01-15
    [HttpGet("by-date")]
    public async Task<ActionResult<List<JaugeageDto>>> GetJaugeagesByDate([FromQuery] DateTime date)
    {
        // Ensure date is UTC to avoid PostgreSQL timestamp errors
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        
        var jaugeages = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Where(j => j.DateJaugeage.Date == utcDate.Date)
            .OrderByDescending(j => j.DateJaugeage)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<JaugeageDto>>(jaugeages));
    }

    // GET: api/Jaugeages/by-temoin/5
    [HttpGet("by-temoin/{temoinId}")]
    public async Task<ActionResult<List<JaugeageDto>>> GetJaugeagesByTemoin(int temoinId)
    {
        var jaugeages = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Where(j => j.TemoinID == temoinId)
            .OrderByDescending(j => j.DateJaugeage)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<JaugeageDto>>(jaugeages));
    }

    // GET: api/Jaugeages/latest
    /// <summary>
    /// Gets the most recent jaugeage
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<JaugeageWithDetailsDto>> GetLatestJaugeage()
    {
        var jaugeage = await _context.Jaugeages
            .Include(j => j.Temoin)
            .Include(j => j.JaugeageDetails)
                .ThenInclude(d => d.Reservoir)
            .OrderByDescending(j => j.DateJaugeage)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (jaugeage == null)
            return NotFound("No jaugeage records found");

        var dto = new JaugeageWithDetailsDto
        {
            ID = jaugeage.ID,
            DateJaugeage = jaugeage.DateJaugeage,
            TemoinID = jaugeage.TemoinID,
            NumeroJaugeage = jaugeage.NumeroJaugeage,
            Observations = jaugeage.Observations,
            TemoinNom = jaugeage.Temoin?.Nom,
            AjoutePar = jaugeage.AjoutePar,
            DateCreation = jaugeage.DateCreation,
            ModifiePar = jaugeage.ModifiePar,
            DateModification = jaugeage.DateModification,
            Details = _mapper.Map<List<JaugeageDetailDto>>(jaugeage.JaugeageDetails)
        };

        return Ok(dto);
    }

    // POST: api/Jaugeages
    [HttpPost]
    public async Task<ActionResult<JaugeageDto>> PostJaugeage([FromBody] JaugeageDto dto)
    {
        // Validate Temoin exists
        var temoin = await _context.Employes.FindAsync(dto.TemoinID);
        if (temoin == null)
            return BadRequest("Temoin (employee) not found");

        var entity = _mapper.Map<Jaugeage>(dto);
        entity.DateCreation = DateTime.UtcNow;

        _context.Jaugeages.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(entity).Reference(j => j.Temoin).LoadAsync();

        return CreatedAtAction(nameof(GetJaugeage), new { id = entity.ID }, 
            _mapper.Map<JaugeageDto>(entity));
    }

    // POST: api/Jaugeages/with-details
    /// <summary>
    /// Creates a Jaugeage with all its details in a single transaction.
    /// Volumes are auto-calculated from calibration data if not provided.
    /// </summary>
    [HttpPost("with-details")]
    public async Task<ActionResult<JaugeageWithDetailsDto>> PostJaugeageWithDetails([FromBody] JaugeageWithDetailsDto dto)
    {
        // Validate Temoin exists
        var temoin = await _context.Employes.FindAsync(dto.TemoinID);
        if (temoin == null)
            return BadRequest("Temoin (employee) not found");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Ensure DateJaugeage is UTC
            var dateJaugeageUtc = dto.DateJaugeage.Kind == DateTimeKind.Utc
                ? dto.DateJaugeage
                : DateTime.SpecifyKind(dto.DateJaugeage, DateTimeKind.Utc);

            // Create the Jaugeage
            var jaugeage = new Jaugeage
            {
                DateJaugeage = dateJaugeageUtc,
                TemoinID = dto.TemoinID,
                NumeroJaugeage = dto.NumeroJaugeage,
                Observations = dto.Observations,
                AjoutePar = dto.AjoutePar,
                DateCreation = DateTime.UtcNow
            };

            _context.Jaugeages.Add(jaugeage);
            await _context.SaveChangesAsync();

            // Create the details with auto-calculated volumes
            if (dto.Details != null && dto.Details.Count > 0)
            {
                foreach (var detailDto in dto.Details)
                {
                    // Validate reservoir exists
                    var reservoir = await _context.Reservoirs.FindAsync(detailDto.ReservoirID);
                    if (reservoir == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Reservoir with ID {detailDto.ReservoirID} not found");
                    }

                    // Auto-calculate volume if not provided
                    if (detailDto.VolumeCalcule == 0)
                    {
                        var calculatedVolume = await CalculateVolumeFromCalibration(
                            detailDto.ReservoirID, detailDto.HauteurMesuree);
                        if (calculatedVolume.HasValue)
                        {
                            detailDto.VolumeCalcule = calculatedVolume.Value;
                        }
                    }

                    var detail = new JaugeageDetail
                    {
                        JaugeageID = jaugeage.ID,
                        ReservoirID = detailDto.ReservoirID,
                        HauteurMesuree = detailDto.HauteurMesuree,
                        VolumeCalcule = detailDto.VolumeCalcule,
                        Temperature = detailDto.Temperature,
                        Notes = detailDto.Notes
                    };

                    _context.JaugeageDetails.Add(detail);
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload with all navigation properties
            var result = await _context.Jaugeages
                .Include(j => j.Temoin)
                .Include(j => j.JaugeageDetails)
                    .ThenInclude(d => d.Reservoir)
                .FirstOrDefaultAsync(j => j.ID == jaugeage.ID);

            var resultDto = new JaugeageWithDetailsDto
            {
                ID = result!.ID,
                DateJaugeage = result.DateJaugeage,
                TemoinID = result.TemoinID,
                NumeroJaugeage = result.NumeroJaugeage,
                Observations = result.Observations,
                TemoinNom = result.Temoin?.Nom,
                AjoutePar = result.AjoutePar,
                DateCreation = result.DateCreation,
                Details = _mapper.Map<List<JaugeageDetailDto>>(result.JaugeageDetails)
            };

            return CreatedAtAction(nameof(GetJaugeageWithDetails), new { id = result.ID }, resultDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error creating jaugeage: {ex.Message}");
        }
    }

    // PUT: api/Jaugeages/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutJaugeage(int id, [FromBody] JaugeageDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch");

        var existing = await _context.Jaugeages.FindAsync(id);
        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);
        existing.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Jaugeages/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJaugeage(int id)
    {
        var jaugeage = await _context.Jaugeages
            .Include(j => j.JaugeageDetails)
            .FirstOrDefaultAsync(j => j.ID == id);

        if (jaugeage == null)
            return NotFound();

        // Remove details first (cascade should handle this, but being explicit)
        _context.JaugeageDetails.RemoveRange(jaugeage.JaugeageDetails);
        _context.Jaugeages.Remove(jaugeage);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Helper method to calculate volume from calibration data
    /// </summary>
    private async Task<decimal?> CalculateVolumeFromCalibration(int reservoirId, decimal hauteurCm)
    {
        // Try exact match first
        var exactMatch = await _context.ReservoirCalibrations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ReservoirID == reservoirId && c.HauteurCm == hauteurCm);

        if (exactMatch != null)
            return exactMatch.VolumeLitres;

        // Interpolate
        var lower = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId && c.HauteurCm < hauteurCm)
            .OrderByDescending(c => c.HauteurCm)
            .FirstOrDefaultAsync();

        var upper = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId && c.HauteurCm > hauteurCm)
            .OrderBy(c => c.HauteurCm)
            .FirstOrDefaultAsync();

        if (lower == null && upper == null)
            return null;

        if (lower == null)
            return upper!.VolumeLitres;

        if (upper == null)
            return lower.VolumeLitres;

        // Linear interpolation
        var ratio = (hauteurCm - lower.HauteurCm) / (upper.HauteurCm - lower.HauteurCm);
        return Math.Round(lower.VolumeLitres + ratio * (upper.VolumeLitres - lower.VolumeLitres), 2);
    }
}
