using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VenteServicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public VenteServicesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/VenteServices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VenteServiceDto>>> GetVenteServices()
    {
        var ventes = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .OrderByDescending(v => v.DateVente)
            .AsNoTracking()
            .ToListAsync();

        var dtos = ventes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    // GET: api/VenteServices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<VenteServiceDto>> GetVenteService(int id)
    {
        var vente = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.ID == id);

        if (vente == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(vente));
    }

    // GET: api/VenteServices/bydate?startDate=2024-01-01&endDate=2024-12-31
    [HttpGet("bydate")]
    public async Task<ActionResult<IEnumerable<VenteServiceDto>>> GetVentesByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var ventes = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .Where(v => v.DateVente >= startDate && v.DateVente <= endDate)
            .OrderByDescending(v => v.DateVente)
            .AsNoTracking()
            .ToListAsync();

        var dtos = ventes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    // GET: api/VenteServices/byservice/5
    [HttpGet("byservice/{serviceId}")]
    public async Task<ActionResult<IEnumerable<VenteServiceDto>>> GetVentesByService(int serviceId)
    {
        var ventes = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .Where(v => v.ServiceID == serviceId)
            .OrderByDescending(v => v.DateVente)
            .AsNoTracking()
            .ToListAsync();

        var dtos = ventes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    // GET: api/VenteServices/bycategory/5
    [HttpGet("bycategory/{categoryId}")]
    public async Task<ActionResult<IEnumerable<VenteServiceDto>>> GetVentesByCategory(int categoryId)
    {
        var ventes = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .Where(v => v.Service.ServiceCategorieID == categoryId)
            .OrderByDescending(v => v.DateVente)
            .AsNoTracking()
            .ToListAsync();

        var dtos = ventes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    // GET: api/VenteServices/byclient/5
    [HttpGet("byclient/{clientId}")]
    public async Task<ActionResult<IEnumerable<VenteServiceDto>>> GetVentesByClient(int clientId)
    {
        var ventes = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .Where(v => v.ClientID == clientId)
            .OrderByDescending(v => v.DateVente)
            .AsNoTracking()
            .ToListAsync();

        var dtos = ventes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    // GET: api/VenteServices/search?term=xxx
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<VenteServiceDto>>> SearchVentes([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return await GetVenteServices();
        }

        term = term.ToLower();
        var ventes = await _context.VenteServices
            .Include(v => v.Service)
                .ThenInclude(s => s.ServiceCategorie)
            .Include(v => v.Client)
            .Include(v => v.Employe)
            .Include(v => v.MoyenPaiement)
            .Where(v =>
                (v.NumeroVente != null && v.NumeroVente.ToLower().Contains(term)) ||
                (v.Service.Description != null && v.Service.Description.ToLower().Contains(term)) ||
                (v.Service.Numero != null && v.Service.Numero.ToLower().Contains(term)) ||
                (v.Client != null && v.Client.Nom != null && v.Client.Nom.ToLower().Contains(term)) ||
                (v.Notes != null && v.Notes.ToLower().Contains(term)))
            .OrderByDescending(v => v.DateVente)
            .AsNoTracking()
            .ToListAsync();

        var dtos = ventes.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    // POST: api/VenteServices
    [HttpPost]
    public async Task<ActionResult<VenteServiceDto>> PostVenteService(VenteServiceDto dto)
    {
        try
        {
            // Ensure DateVente is UTC for PostgreSQL compatibility
            var dateVente = dto.DateVente.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dto.DateVente, DateTimeKind.Utc)
                : dto.DateVente.ToUniversalTime();

            var entity = new VenteService
            {
                NumeroVente = dto.NumeroVente,
                DateVente = dateVente,
                ServiceID = dto.ServiceID,
                Quantite = dto.Quantite,
                PrixUnitaire = dto.PrixUnitaire,
                ClientID = dto.ClientID,
                EmployeID = dto.EmployeID,
                MoyenPaiementID = dto.MoyenPaiementID,
                Notes = dto.Notes,
                Statut = dto.Statut ?? "Confirmee",
                DateCreation = DateTime.UtcNow,
                CreePar = dto.CreePar
            };

            // Generate sale number if not provided
            if (string.IsNullOrEmpty(entity.NumeroVente))
            {
                entity.NumeroVente = entity.GenerateNumeroVente();
            }

            _context.VenteServices.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with related entities
            await _context.Entry(entity).Reference(e => e.Service).LoadAsync();
            if (entity.Service != null)
            {
                await _context.Entry(entity.Service).Reference(s => s.ServiceCategorie).LoadAsync();
            }
            await _context.Entry(entity).Reference(e => e.Client).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Employe).LoadAsync();
            await _context.Entry(entity).Reference(e => e.MoyenPaiement).LoadAsync();

            var resultDto = MapToDto(entity);
            return CreatedAtAction(nameof(GetVenteService), new { id = entity.ID }, resultDto);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating service sale: {ex.Message}");
        }
    }

    // PUT: api/VenteServices/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutVenteService(int id, VenteServiceDto dto)
    {
        if (id != dto.ID)
        {
            return BadRequest("ID mismatch.");
        }

        var existing = await _context.VenteServices.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        // Ensure DateVente is UTC for PostgreSQL compatibility
        var dateVente = dto.DateVente.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dto.DateVente, DateTimeKind.Utc)
            : dto.DateVente.ToUniversalTime();

        existing.NumeroVente = dto.NumeroVente;
        existing.DateVente = dateVente;
        existing.ServiceID = dto.ServiceID;
        existing.Quantite = dto.Quantite;
        existing.PrixUnitaire = dto.PrixUnitaire;
        existing.ClientID = dto.ClientID;
        existing.EmployeID = dto.EmployeID;
        existing.MoyenPaiementID = dto.MoyenPaiementID;
        existing.Notes = dto.Notes;
        existing.Statut = dto.Statut;
        existing.DateModification = DateTime.UtcNow;
        existing.ModifiePar = dto.ModifiePar;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!VenteServiceExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/VenteServices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVenteService(int id)
    {
        var vente = await _context.VenteServices.FindAsync(id);
        if (vente == null)
        {
            return NotFound();
        }

        _context.VenteServices.Remove(vente);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool VenteServiceExists(int id)
    {
        return _context.VenteServices.Any(e => e.ID == id);
    }

    private static VenteServiceDto MapToDto(VenteService entity)
    {
        return new VenteServiceDto
        {
            ID = entity.ID,
            NumeroVente = entity.NumeroVente,
            DateVente = entity.DateVente,
            ServiceID = entity.ServiceID,
            ServiceNumero = entity.Service?.Numero,
            ServiceDescription = entity.Service?.Description,
            ServiceCategorieNom = entity.Service?.ServiceCategorie?.Nom,
            Quantite = entity.Quantite,
            PrixUnitaire = entity.PrixUnitaire,
            MontantTotal = entity.MontantTotal,
            ClientID = entity.ClientID,
            ClientNom = entity.Client?.Nom,
            EmployeID = entity.EmployeID,
            EmployeNom = entity.Employe?.Nom,
            MoyenPaiementID = entity.MoyenPaiementID,
            MoyenPaiementNom = entity.MoyenPaiement?.Nom,
            Notes = entity.Notes,
            Statut = entity.Statut,
            DateCreation = entity.DateCreation,
            DateModification = entity.DateModification,
            CreePar = entity.CreePar,
            ModifiePar = entity.ModifiePar
        };
    }
}
