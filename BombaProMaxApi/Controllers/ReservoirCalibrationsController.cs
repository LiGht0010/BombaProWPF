using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservoirCalibrationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ReservoirCalibrationsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all calibration entries for a specific reservoir
    /// </summary>
    [HttpGet("reservoir/{reservoirId}")]
    public async Task<ActionResult<List<ReservoirCalibrationDto>>> GetCalibrationsByReservoir(int reservoirId)
    {
        var calibrations = await _context.ReservoirCalibrations
            .Include(c => c.Reservoir)
            .Where(c => c.ReservoirID == reservoirId)
            .OrderBy(c => c.HauteurCm)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ReservoirCalibrationDto>>(calibrations));
    }

    /// <summary>
    /// Get a specific calibration entry
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservoirCalibrationDto>> GetCalibration(int id)
    {
        var calibration = await _context.ReservoirCalibrations
            .Include(c => c.Reservoir)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ID == id);

        if (calibration == null)
            return NotFound();

        return Ok(_mapper.Map<ReservoirCalibrationDto>(calibration));
    }

    /// <summary>
    /// Look up volume for a given height in a reservoir.
    /// If exact height not found, interpolates between nearest values.
    /// </summary>
    [HttpGet("reservoir/{reservoirId}/lookup")]
    public async Task<ActionResult<VolumeLookupResultDto>> LookupVolume(int reservoirId, [FromQuery] decimal hauteurCm)
    {
        var reservoir = await _context.Reservoirs
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ID == reservoirId);

        if (reservoir == null)
            return NotFound("Reservoir not found");

        // Try exact match first (with small tolerance for decimal precision)
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

    /// <summary>
    /// Create a single calibration entry
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReservoirCalibrationDto>> CreateCalibration([FromBody] ReservoirCalibrationDto dto)
    {
        // Check if reservoir exists
        var reservoir = await _context.Reservoirs.FindAsync(dto.ReservoirID);
        if (reservoir == null)
            return BadRequest("Reservoir not found");

        // Check for duplicate height entry
        var exists = await _context.ReservoirCalibrations
            .AnyAsync(c => c.ReservoirID == dto.ReservoirID && c.HauteurCm == dto.HauteurCm);
        
        if (exists)
            return Conflict($"Calibration entry for height {dto.HauteurCm} cm already exists");

        var entity = _mapper.Map<ReservoirCalibration>(dto);
        _context.ReservoirCalibrations.Add(entity);
        await _context.SaveChangesAsync();

        // Update reservoir's HauteurMax if this is the highest entry
        await UpdateReservoirHauteurMax(dto.ReservoirID);

        await _context.Entry(entity).Reference(c => c.Reservoir).LoadAsync();
        return CreatedAtAction(nameof(GetCalibration), new { id = entity.ID }, _mapper.Map<ReservoirCalibrationDto>(entity));
    }

    /// <summary>
    /// Bulk import calibration data for a reservoir (replaces existing data)
    /// </summary>
    [HttpPost("reservoir/{reservoirId}/import")]
    public async Task<ActionResult> ImportCalibrations(int reservoirId, [FromBody] List<ReservoirCalibrationImportDto> calibrations)
    {
        // Check if reservoir exists
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir == null)
            return NotFound("Reservoir not found");

        if (calibrations == null || calibrations.Count == 0)
            return BadRequest("No calibration data provided");

        // Validate data
        var duplicateHeights = calibrations.GroupBy(c => c.HauteurCm).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateHeights.Count != 0)
            return BadRequest($"Duplicate height entries found: {string.Join(", ", duplicateHeights)}");

        // Remove existing calibrations for this reservoir
        var existingCalibrations = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId)
            .ToListAsync();
        
        _context.ReservoirCalibrations.RemoveRange(existingCalibrations);

        // Add new calibrations
        var newCalibrations = calibrations.Select(c => new ReservoirCalibration
        {
            ReservoirID = reservoirId,
            HauteurCm = c.HauteurCm,
            VolumeLitres = c.VolumeLitres
        }).ToList();

        _context.ReservoirCalibrations.AddRange(newCalibrations);
        await _context.SaveChangesAsync();

        // Update reservoir's HauteurMax
        await UpdateReservoirHauteurMax(reservoirId);

        return Ok(new { 
            Message = $"Successfully imported {newCalibrations.Count} calibration entries",
            Count = newCalibrations.Count,
            HauteurMax = newCalibrations.Max(c => c.HauteurCm)
        });
    }

    /// <summary>
    /// Update a calibration entry
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCalibration(int id, [FromBody] ReservoirCalibrationDto dto)
    {
        if (id != dto.ID)
            return BadRequest("ID mismatch");

        var existing = await _context.ReservoirCalibrations.FindAsync(id);
        if (existing == null)
            return NotFound();

        // Check for duplicate height (if height changed)
        if (existing.HauteurCm != dto.HauteurCm)
        {
            var duplicateExists = await _context.ReservoirCalibrations
                .AnyAsync(c => c.ReservoirID == existing.ReservoirID && c.HauteurCm == dto.HauteurCm && c.ID != id);
            
            if (duplicateExists)
                return Conflict($"Calibration entry for height {dto.HauteurCm} cm already exists");
        }

        existing.HauteurCm = dto.HauteurCm;
        existing.VolumeLitres = dto.VolumeLitres;

        await _context.SaveChangesAsync();
        await UpdateReservoirHauteurMax(existing.ReservoirID);

        return NoContent();
    }

    /// <summary>
    /// Delete a calibration entry
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCalibration(int id)
    {
        var calibration = await _context.ReservoirCalibrations.FindAsync(id);
        if (calibration == null)
            return NotFound();

        var reservoirId = calibration.ReservoirID;
        _context.ReservoirCalibrations.Remove(calibration);
        await _context.SaveChangesAsync();

        await UpdateReservoirHauteurMax(reservoirId);

        return NoContent();
    }

    /// <summary>
    /// Delete all calibration entries for a reservoir
    /// </summary>
    [HttpDelete("reservoir/{reservoirId}")]
    public async Task<IActionResult> DeleteAllCalibrations(int reservoirId)
    {
        var calibrations = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId)
            .ToListAsync();

        if (calibrations.Count == 0)
            return NotFound("No calibrations found for this reservoir");

        _context.ReservoirCalibrations.RemoveRange(calibrations);
        
        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.HauteurMax = null;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Deleted {calibrations.Count} calibration entries" });
    }

    /// <summary>
    /// Updates the HauteurMax field on the reservoir based on calibration data
    /// </summary>
    private async Task UpdateReservoirHauteurMax(int reservoirId)
    {
        var maxHeight = await _context.ReservoirCalibrations
            .Where(c => c.ReservoirID == reservoirId)
            .MaxAsync(c => (decimal?)c.HauteurCm);

        var reservoir = await _context.Reservoirs.FindAsync(reservoirId);
        if (reservoir != null)
        {
            reservoir.HauteurMax = maxHeight;
            await _context.SaveChangesAsync();
        }
    }
}
