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

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenteLubrifiantsEtArticlesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public VenteLubrifiantsEtArticlesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/VenteLubrifiantsEtArticles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenteLubrifiantsEtArticlesDto>>> GetVenteLubrifiantsEtArticles()
        {
            var ventes = await _context.VenteLubrifiantsEtArticles
                .Include(v => v.Produit)
                    .ThenInclude(p => p.Categorie)
                .Include(v => v.Client)
                .Include(v => v.Employe)
                .Include(v => v.MoyenPaiement)
                .OrderByDescending(v => v.DateVente)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<VenteLubrifiantsEtArticlesDto>>(ventes);
            
            // Calculate computed properties
            foreach (var dto in dtos)
            {
                CalculateComputedProperties(dto, ventes.FirstOrDefault(v => v.ID == dto.ID));
            }

            return Ok(dtos);
        }

        // GET: api/VenteLubrifiantsEtArticles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VenteLubrifiantsEtArticlesDto>> GetVenteLubrifiantsEtArticles(int id)
        {
            var vente = await _context.VenteLubrifiantsEtArticles
                .Include(v => v.Produit)
                    .ThenInclude(p => p.Categorie)
                .Include(v => v.Client)
                .Include(v => v.Employe)
                .Include(v => v.MoyenPaiement)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.ID == id);

            if (vente == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<VenteLubrifiantsEtArticlesDto>(vente);
            CalculateComputedProperties(dto, vente);

            return Ok(dto);
        }

        // GET: api/VenteLubrifiantsEtArticles/bydate?startDate=2024-01-01&endDate=2024-12-31
        [HttpGet("bydate")]
        public async Task<ActionResult<IEnumerable<VenteLubrifiantsEtArticlesDto>>> GetVentesByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var ventes = await _context.VenteLubrifiantsEtArticles
                .Include(v => v.Produit)
                    .ThenInclude(p => p.Categorie)
                .Include(v => v.Client)
                .Include(v => v.Employe)
                .Include(v => v.MoyenPaiement)
                .Where(v => v.DateVente >= startDate && v.DateVente <= endDate)
                .OrderByDescending(v => v.DateVente)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<VenteLubrifiantsEtArticlesDto>>(ventes);
            
            foreach (var dto in dtos)
            {
                CalculateComputedProperties(dto, ventes.FirstOrDefault(v => v.ID == dto.ID));
            }

            return Ok(dtos);
        }

        // GET: api/VenteLubrifiantsEtArticles/bycategory/{categoryName}
        [HttpGet("bycategory/{categoryName}")]
        public async Task<ActionResult<IEnumerable<VenteLubrifiantsEtArticlesDto>>> GetVentesByCategory(string categoryName)
        {
            var ventes = await _context.VenteLubrifiantsEtArticles
                .Include(v => v.Produit)
                    .ThenInclude(p => p.Categorie)
                .Include(v => v.Client)
                .Include(v => v.Employe)
                .Include(v => v.MoyenPaiement)
                .Where(v => v.Produit.Categorie != null && 
                           v.Produit.Categorie.Nom.ToLower() == categoryName.ToLower())
                .OrderByDescending(v => v.DateVente)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<VenteLubrifiantsEtArticlesDto>>(ventes);
            
            foreach (var dto in dtos)
            {
                CalculateComputedProperties(dto, ventes.FirstOrDefault(v => v.ID == dto.ID));
            }

            return Ok(dtos);
        }

        // PUT: api/VenteLubrifiantsEtArticles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenteLubrifiantsEtArticles(int id, VenteLubrifiantsEtArticlesDto dto)
        {
            if (id != dto.ID)
            {
                return BadRequest("ID mismatch.");
            }

            var existing = await _context.VenteLubrifiantsEtArticles.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Map DTO to entity
            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenteLubrifiantsEtArticlesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/VenteLubrifiantsEtArticles
        [HttpPost]
        public async Task<ActionResult<VenteLubrifiantsEtArticlesDto>> PostVenteLubrifiantsEtArticles(VenteLubrifiantsEtArticlesDto dto)
        {
            try
            {
                // Create entity manually to ensure proper mapping
                var entity = new VenteLubrifiantsEtArticles
                {
                    NumeroVente = dto.NumeroVente,
                    DateVente = dto.DateVente,
                    ProduitID = dto.ProduitID,
                    QuantiteVendue = dto.QuantiteVendue,
                    PrixUnitaireTTC = dto.PrixUnitaireTTC,
                    ClientID = dto.ClientID,
                    EmployeID = dto.EmployeID,
                    MoyenPaiementID = dto.MoyenPaiementID,
                    Notes = dto.Notes,
                    Statut = dto.Statut ?? "Confirmée",
                    DateCreation = DateTime.UtcNow,
                    CreePar = dto.CreePar
                };

                // Generate sale number if not provided
                if (string.IsNullOrEmpty(entity.NumeroVente))
                {
                    entity.NumeroVente = GenerateNumeroVente(entity.DateVente);
                }

                _context.VenteLubrifiantsEtArticles.Add(entity);
                await _context.SaveChangesAsync();

                // Update product stock
                var produit = await _context.Produits.FindAsync(entity.ProduitID);
                if (produit != null && produit.Stock.HasValue)
                {
                    produit.Stock -= entity.QuantiteVendue;
                    await _context.SaveChangesAsync();
                }

                // Reload with related entities for response
                await _context.Entry(entity).Reference(e => e.Produit).LoadAsync();
                if (entity.Produit != null)
                {
                    await _context.Entry(entity.Produit).Reference(p => p.Categorie).LoadAsync();
                }
                await _context.Entry(entity).Reference(e => e.Client).LoadAsync();
                await _context.Entry(entity).Reference(e => e.Employe).LoadAsync();
                await _context.Entry(entity).Reference(e => e.MoyenPaiement).LoadAsync();

                var resultDto = _mapper.Map<VenteLubrifiantsEtArticlesDto>(entity);
                CalculateComputedProperties(resultDto, entity);

                return CreatedAtAction(nameof(GetVenteLubrifiantsEtArticles), new { id = entity.ID }, resultDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating sale: {ex.Message}");
            }
        }

        // DELETE: api/VenteLubrifiantsEtArticles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenteLubrifiantsEtArticles(int id)
        {
            var vente = await _context.VenteLubrifiantsEtArticles.FindAsync(id);
            if (vente == null)
            {
                return NotFound();
            }

            // Restore product stock
            var produit = await _context.Produits.FindAsync(vente.ProduitID);
            if (produit != null && produit.Stock.HasValue)
            {
                produit.Stock += vente.QuantiteVendue;
            }

            _context.VenteLubrifiantsEtArticles.Remove(vente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VenteLubrifiantsEtArticlesExists(int id)
        {
            return _context.VenteLubrifiantsEtArticles.Any(e => e.ID == id);
        }

        private static string GenerateNumeroVente(DateTime dateVente)
        {
            var date = dateVente.ToString("yyyyMMdd");
            var time = dateVente.ToString("HHmmss");
            return $"VLA-{date}-{time}";
        }

        private static void CalculateComputedProperties(VenteLubrifiantsEtArticlesDto dto, VenteLubrifiantsEtArticles? entity)
        {
            const decimal TVA_RATE = 0.2M;
            
            dto.PrixUnitaireHT = dto.PrixUnitaireTTC / (1 + TVA_RATE);
            dto.MontantTotalHT = dto.PrixUnitaireHT * dto.QuantiteVendue;
            dto.MontantTotalTTC = dto.PrixUnitaireTTC * dto.QuantiteVendue;
            dto.MontantTVA = dto.MontantTotalTTC - dto.MontantTotalHT;

            if (entity?.Produit != null)
            {
                dto.CategorieNom = entity.Produit.Categorie?.Nom ?? "Non définie";
                
                if (entity.Produit.PrixAchat.HasValue)
                {
                    var coutTotal = entity.Produit.PrixAchat.Value * dto.QuantiteVendue;
                    dto.MargeBeneficiaire = dto.MontantTotalHT - coutTotal;
                    
                    if (entity.Produit.PrixAchat.Value > 0)
                    {
                        var margeUnitaire = dto.PrixUnitaireHT - entity.Produit.PrixAchat.Value;
                        dto.TauxMarge = (margeUnitaire / entity.Produit.PrixAchat.Value) * 100;
                    }
                }
            }
        }
    }
}
