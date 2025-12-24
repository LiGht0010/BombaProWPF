using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JaugeageDetailsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public JaugeageDetailsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/JaugeageDetails
    [HttpGet]
    public async Task<ActionResult<List<JaugeageDetailDto>>> GetJaugeageDetails()
    {
        var details = await _context.JaugeageDetails
            .Include(d => d.Jaugeage)
            .Include(d => d.Reservoir)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<JaugeageDetailDto>>(details));
    }

    // GET: api/JaugeageDetails/5
    [HttpGet("{id}")]
    public async Task<ActionResult<JaugeageDetailDto>> GetJaugeageDetail(int id)
    {
        var detail = await _context.JaugeageDetails
            .Include(d => d.Jaugeage)
            .Include(d => d.Reservoir)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ID == id);

        if (detail == null)
            return NotFound();

        return Ok(_mapper.Map<JaugeageDetailDto>(detail));
    }

    // GET: api/JaugeageDetails/jaugeage/5
    [HttpGet("jaugeage/{jaugeageId}")]
    public async Task<ActionResult<List<JaugeageDetailDto>>> GetDetailsByJaugeage(int jaugeageId)
    {
        var details = await _context.JaugeageDetails
            .Include(d => d.Jaugeage)
            .Include(d => d.Reservoir)
            .Where(d => d.JaugeageID == jaugeageId)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<JaugeageDetailDto>>(details));
    }

    // GET: api/JaugeageDetails/reservoir/5
    [HttpGet("reservoir/{reservoirId}")]
    public async Task<ActionResult<List<JaugeageDetailDto>>> GetDetailsByReservoir(int reservoirId)
    {
        var details = await _context.JaugeageDetails
            .Include(d => d.Jaugeage)
            .Include(d => d.Reservoir)
            .Where(d => d.ReservoirID == reservoirId)
            .OrderByDescending(d => d.Jaugeage!.DateJaugeage)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<JaugeageDetailDto>>(details));
    }

    // GET: api/JaugeageDetails/calculate-volume?reservoirId=1&hauteurCm=201
    /// <summary>
    /// Calculates volume from height using reservoir calibration data.
    /// Returns the calculated volume or null if no calibration data exists.
    /// </summary>
    [HttpGet("calculate-volume")]
    public async Task<ActionResult<VolumeLookupResultDto>> CalculateVolume(
        [FromQuery] int reservoirId, 
        [FromQuery] decimal hauteurCm)
    {
        var reservoir = await _context.Reservoirs
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ID == reservoirId);

        if (reservoir == null)
            return NotFound("Reservoir not found");

        // Try exact match first
        var exactMatch = await _context.ReservoirCalibrations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ReservoirID == reservoirId && c.HauteurCm == hauteurCm);

        if (exactMatch != null)
        {
            return Ok(new VolumeLookupResultDto
            {
                ReservoirID = reservoirId,
                ReservoirNumero = reservoir.Numero,
                HauteurCm = hauteurCm,
                VolumeLitres = exactMatch.VolumeLitres,
                IsInterpolated = false
            });
        }

        // Interpolate between nearest values
        var lower = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId && c.HauteurCm < hauteurCm)
            .OrderByDescending(c => c.HauteurCm)
            .FirstOrDefaultAsync();

        var upper = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId && c.HauteurCm > hauteurCm)
            .OrderBy(c => c.HauteurCm)
            .FirstOrDefaultAsync();

        if (lower == null && upper == null)
        {
            return NotFound("No calibration data available for this reservoir");
        }

        decimal interpolatedVolume;
        if (lower == null)
        {
            interpolatedVolume = upper!.VolumeLitres;
        }
        else if (upper == null)
        {
            interpolatedVolume = lower.VolumeLitres;
        }
        else
        {
            // Linear interpolation
            var ratio = (hauteurCm - lower.HauteurCm) / (upper.HauteurCm - lower.HauteurCm);
            interpolatedVolume = lower.VolumeLitres + ratio * (upper.VolumeLitres - lower.VolumeLitres);
        }

        return Ok(new VolumeLookupResultDto
        {
            ReservoirID = reservoirId,
            ReservoirNumero = reservoir.Numero,
            HauteurCm = hauteurCm,
            VolumeLitres = Math.Round(interpolatedVolume, 2),
            IsInterpolated = true
        });
    }

    // POST: api/JaugeageDetails
    /// <summary>
    /// Creates a new JaugeageDetail. If VolumeCalcule is 0 or not provided,
    /// it will be auto-calculated from the reservoir's calibration data.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JaugeageDetailDto>> PostJaugeageDetail([FromBody] JaugeageDetailDto dto)
    {
        // Validate reservoir exists
        var reservoir = await _context.Reservoirs.FindAsync(dto.ReservoirID);
        if (reservoir == null)
            return BadRequest("Reservoir not found");

        // Validate jaugeage exists
        var jaugeage = await _context.Jaugeages.FindAsync(dto.JaugeageID);
        if (jaugeage == null)
            return BadRequest("Jaugeage not found");

        // Auto-calculate volume if not provided or is 0
        if (dto.VolumeCalcule == 0)
        {
            var calculatedVolume = await CalculateVolumeFromCalibration(dto.ReservoirID, dto.HauteurMesuree);
            if (calculatedVolume.HasValue)
            {
                dto.VolumeCalcule = calculatedVolume.Value;
            }
        }

        var entity = _mapper.Map<JaugeageDetail>(dto);
        _context.JaugeageDetails.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(entity).Reference(d => d.Jaugeage).LoadAsync();
        await _context.Entry(entity).Reference(d => d.Reservoir).LoadAsync();

        return CreatedAtAction(nameof(GetJaugeageDetail), new { id = entity.ID }, 
            _mapper.Map<JaugeageDetailDto>(entity));
    }

    // POST: api/JaugeageDetails/with-auto-volume
    /// <summary>
    /// Creates a JaugeageDetail with automatic volume calculation from calibration.
    /// Always calculates volume from calibration data, ignoring any provided VolumeCalcule.
    /// </summary>
    [HttpPost("with-auto-volume")]
    public async Task<ActionResult<JaugeageDetailDto>> PostJaugeageDetailWithAutoVolume([FromBody] JaugeageDetailDto dto)
    {
        // Validate reservoir exists
        var reservoir = await _context.Reservoirs.FindAsync(dto.ReservoirID);
        if (reservoir == null)
            return BadRequest("Reservoir not found");

        // Validate jaugeage exists
        var jaugeage = await _context.Jaugeages.FindAsync(dto.JaugeageID);
        if (jaugeage == null)
            return BadRequest("Jaugeage not found");

        // Always calculate volume from calibration
        var calculatedVolume = await CalculateVolumeFromCalibration(dto.ReservoirID, dto.HauteurMesuree);
        if (!calculatedVolume.HasValue)
        {
            return BadRequest($"No calibration data available for reservoir {reservoir.Numero}. Please import calibration data first.");
        }

        dto.VolumeCalcule = calculatedVolume.Value;

        var entity = _mapper.Map<JaugeageDetail>(dto);
        _context.JaugeageDetails.Add(entity);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(entity).Reference(d => d.Jaugeage).LoadAsync();
        await _context.Entry(entity).Reference(d => d.Reservoir).LoadAsync();

        return CreatedAtAction(nameof(GetJaugeageDetail), new { id = entity.ID }, 
            _mapper.Map<JaugeageDetailDto>(entity));
    }

    // PUT: api/JaugeageDetails/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutJaugeageDetail(int id, [FromBody] JaugeageDetailDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch");

        var existing = await _context.JaugeageDetails.FindAsync(id);
        if (existing == null)
            return NotFound();

        // If height changed and volume is 0, recalculate
        if (existing.HauteurMesuree != dto.HauteurMesuree && dto.VolumeCalcule == 0)
        {
            var calculatedVolume = await CalculateVolumeFromCalibration(dto.ReservoirID, dto.HauteurMesuree);
            if (calculatedVolume.HasValue)
            {
                dto.VolumeCalcule = calculatedVolume.Value;
            }
        }

        _mapper.Map(dto, existing);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/JaugeageDetails/5/recalculate
    /// <summary>
    /// Recalculates the volume for an existing JaugeageDetail based on current calibration data.
    /// </summary>
    [HttpPut("{id}/recalculate")]
    public async Task<ActionResult<JaugeageDetailDto>> RecalculateVolume(int id)
    {
        var detail = await _context.JaugeageDetails
            .Include(d => d.Jaugeage)
            .Include(d => d.Reservoir)
            .FirstOrDefaultAsync(d => d.ID == id);

        if (detail == null)
            return NotFound();

        var calculatedVolume = await CalculateVolumeFromCalibration(detail.ReservoirID, detail.HauteurMesuree);
        if (!calculatedVolume.HasValue)
        {
            return BadRequest("No calibration data available for this reservoir");
        }

        detail.VolumeCalcule = calculatedVolume.Value;
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<JaugeageDetailDto>(detail));
    }

    // DELETE: api/JaugeageDetails/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJaugeageDetail(int id)
    {
        var detail = await _context.JaugeageDetails.FindAsync(id);
        if (detail == null)
            return NotFound();

        _context.JaugeageDetails.Remove(detail);
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
