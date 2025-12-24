using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchatsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AchatsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/achats
        [HttpGet]
        public async Task<ActionResult<List<AchatDto>>> GetAchats()
        {
            var achats = await _context.Achats
                .Include(a => a.Fournisseur)
                .Include(a => a.Produit)
                .Include(a => a.Chauffeur)
                .Include(a => a.Camion)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatDto>>(achats);
            return Ok(dtos);
        }

        // GET: api/achats/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AchatDto>> GetAchat(int id)
        {
            var achat = await _context.Achats
                .Include(a => a.Fournisseur)
                .Include(a => a.Produit)
                .Include(a => a.Chauffeur)
                .Include(a => a.Camion)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == id);

            if (achat == null)
                return NotFound();

            var dto = _mapper.Map<AchatDto>(achat);
            return Ok(dto);
        }

        // GET: api/achats/fournisseur/5
        [HttpGet("fournisseur/{fournisseurId}")]
        public async Task<ActionResult<List<AchatDto>>> GetAchatsByFournisseur(int fournisseurId)
        {
            var achats = await _context.Achats
                .Include(a => a.Fournisseur)
                .Include(a => a.Produit)
                .Include(a => a.Chauffeur)
                .Include(a => a.Camion)
                .Where(a => a.FournisseurID == fournisseurId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatDto>>(achats);
            return Ok(dtos);
        }

        // GET: api/achats/produit/5
        [HttpGet("produit/{produitId}")]
        public async Task<ActionResult<List<AchatDto>>> GetAchatsByProduit(int produitId)
        {
            var achats = await _context.Achats
                .Include(a => a.Fournisseur)
                .Include(a => a.Produit)
                .Include(a => a.Chauffeur)
                .Include(a => a.Camion)
                .Where(a => a.ProduitID == produitId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatDto>>(achats);
            return Ok(dtos);
        }

        // GET: api/achats/daterange?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("daterange")]
        public async Task<ActionResult<List<AchatDto>>> GetAchatsByDateRange(
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            var achats = await _context.Achats
                .Include(a => a.Fournisseur)
                .Include(a => a.Produit)
                .Include(a => a.Chauffeur)
                .Include(a => a.Camion)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<AchatDto>>(achats);
            return Ok(dtos);
        }

        // POST: api/achats
        [HttpPost]
        public async Task<ActionResult<AchatDto>> CreateAchat([FromBody] AchatDto dto)
        {
            var entity = _mapper.Map<Achat>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Achats.Add(entity);
            await _context.SaveChangesAsync();

            // Load product with category for stock update
            await _context.Entry(entity).Reference(e => e.Produit).LoadAsync();
            if (entity.Produit != null)
            {
                await _context.Entry(entity.Produit).Reference(p => p.Categorie).LoadAsync();
                entity.UpdateProductStock();
                await _context.SaveChangesAsync();
            }

            // Reload with related entities for response mapping
            await _context.Entry(entity).Reference(e => e.Fournisseur).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Chauffeur).LoadAsync();
            await _context.Entry(entity).Reference(e => e.Camion).LoadAsync();

            var resultDto = _mapper.Map<AchatDto>(entity);
            return CreatedAtAction(nameof(GetAchat), new { id = entity.ID }, resultDto);
        }

        // PUT: api/achats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAchat(int id, [FromBody] AchatDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Achats
                .Include(a => a.Produit)
                    .ThenInclude(p => p!.Categorie)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (existing == null)
                return NotFound();

            // Reverse old stock before updating
            existing.ReverseProductStock();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            // Apply new stock after updating
            if (existing.Produit != null)
            {
                existing.UpdateProductStock();
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/achats/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchat(int id)
        {
            var achat = await _context.Achats
                .Include(a => a.Produit)
                    .ThenInclude(p => p!.Categorie)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (achat == null)
                return NotFound();

            // Reverse stock before deleting
            achat.ReverseProductStock();

            _context.Achats.Remove(achat);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}