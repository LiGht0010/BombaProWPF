using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProduitsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProduitsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Produits
        [HttpGet]
        public async Task<ActionResult<List<ProduitDto>>> GetProduits()
        {
            var produits = await _context.Produits
                .Include(p => p.Categorie)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ProduitDto>>(produits);
            return Ok(dtos);
        }

        // GET: api/Produits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProduitDto>> GetProduit(int id)
        {
            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == id);

            if (produit == null)
                return NotFound();

            var dto = _mapper.Map<ProduitDto>(produit);
            return Ok(dto);
        }

        // GET: api/Produits/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<List<ProduitDto>>> GetProduitsByCategory(int categoryId)
        {
            var produits = await _context.Produits
                .Include(p => p.Categorie)
                .Where(p => p.CategorieID == categoryId)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ProduitDto>>(produits);
            return Ok(dtos);
        }

        // GET: api/Produits/lowstock
        [HttpGet("lowstock")]
        public async Task<ActionResult<List<ProduitDto>>> GetLowStockProduits()
        {
            var produits = await _context.Produits
                .Include(p => p.Categorie)
                .Where(p => p.Stock.HasValue && p.StockMinimum.HasValue && p.Stock <= p.StockMinimum)
                .AsNoTracking()
                .ToListAsync();

            var dtos = _mapper.Map<List<ProduitDto>>(produits);
            return Ok(dtos);
        }

        // POST: api/Produits
        [HttpPost]
        public async Task<ActionResult<ProduitDto>> CreateProduit([FromBody] ProduitDto dto)
        {
            var entity = _mapper.Map<Produit>(dto);
            entity.DateCreation = DateTime.UtcNow;

            // Calculate PrixTTC if needed
            entity.CalculatePrixTTC();

            _context.Produits.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with Categorie for response
            if (entity.CategorieID.HasValue)
            {
                await _context.Entry(entity).Reference(p => p.Categorie).LoadAsync();
            }

            var resultDto = _mapper.Map<ProduitDto>(entity);
            return CreatedAtAction(nameof(GetProduit), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Produits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduit(int id, [FromBody] ProduitDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Produits.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            // Calculate PrixTTC if needed
            existing.CalculatePrixTTC();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Produits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduit(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
                return NotFound();

            _context.Produits.Remove(produit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
