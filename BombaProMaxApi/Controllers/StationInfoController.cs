using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StationInfoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public StationInfoController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets the station information (singleton record).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StationInfoDto>> GetStationInfo()
    {
        var stationInfo = await _context.StationInfos
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (stationInfo == null)
        {
            // Return empty default if not configured yet
            return Ok(new StationInfoDto { StationName = "Station Service" });
        }

        var dto = _mapper.Map<StationInfoDto>(stationInfo);
        return Ok(dto);
    }

    /// <summary>
    /// Gets the station information by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StationInfoDto>> GetStationInfoById(int id)
    {
        var stationInfo = await _context.StationInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ID == id);

        if (stationInfo == null)
            return NotFound();

        var dto = _mapper.Map<StationInfoDto>(stationInfo);
        return Ok(dto);
    }

    /// <summary>
    /// Creates station information (only one record allowed per tenant).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StationInfoDto>> CreateStationInfo([FromBody] StationInfoDto dto)
    {
        // Check if already exists
        var existing = await _context.StationInfos.FirstOrDefaultAsync();
        if (existing != null)
        {
            return BadRequest("Station info already exists. Use PUT to update.");
        }

        var entity = _mapper.Map<StationInfo>(dto);
        entity.DateModification = DateTime.UtcNow;

        _context.StationInfos.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<StationInfoDto>(entity);
        return CreatedAtAction(nameof(GetStationInfoById), new { id = entity.ID }, resultDto);
    }

    /// <summary>
    /// Updates station information.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StationInfoDto>> UpdateStationInfo(int id, [FromBody] StationInfoDto dto)
    {
        var existing = await _context.StationInfos.FindAsync(id);
        if (existing == null)
            return NotFound();

        // Map properties but preserve ID
        _mapper.Map(dto, existing);
        existing.ID = id;
        existing.DateModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<StationInfoDto>(existing);
        return Ok(resultDto);
    }

    /// <summary>
    /// Updates only the logo.
    /// </summary>
    [HttpPatch("logo")]
    public async Task<IActionResult> UpdateLogo([FromBody] LogoUpdateDto logoDto)
    {
        var existing = await _context.StationInfos.FirstOrDefaultAsync();
        if (existing == null)
        {
            // Create a minimal record with just the logo
            existing = new StationInfo
            {
                StationName = "Station Service",
                Logo = !string.IsNullOrEmpty(logoDto.LogoBase64) 
                    ? Convert.FromBase64String(logoDto.LogoBase64) 
                    : null,
                DateModification = DateTime.UtcNow
            };
            _context.StationInfos.Add(existing);
        }
        else
        {
            existing.Logo = !string.IsNullOrEmpty(logoDto.LogoBase64) 
                ? Convert.FromBase64String(logoDto.LogoBase64) 
                : null;
            existing.DateModification = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Deletes station information.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStationInfo(int id)
    {
        var stationInfo = await _context.StationInfos.FindAsync(id);
        if (stationInfo == null)
            return NotFound();

        _context.StationInfos.Remove(stationInfo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

/// <summary>
/// DTO for logo-only updates.
/// </summary>
public class LogoUpdateDto
{
    public string? LogoBase64 { get; set; }
}
