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
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClientsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<List<ClientDto>>> GetClients()
        {
            var clients = await _context.Clients
                .AsNoTracking()
                .OrderByDescending(c => c.DateCreation)
                .ToListAsync();

            var dtos = _mapper.Map<List<ClientDto>>(clients);
            return Ok(dtos);
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == id);

            if (client == null)
                return NotFound();

            var dto = _mapper.Map<ClientDto>(client);
            return Ok(dto);
        }

        // GET: api/Clients/search?term=xxx
        [HttpGet("search")]
        public async Task<ActionResult<List<ClientDto>>> SearchClients([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return await GetClients();
            }

            var lowerTerm = term.ToLower();
            var clients = await _context.Clients
                .Where(c => c.Nom.ToLower().Contains(lowerTerm) ||
                           c.NumeroClient.ToLower().Contains(lowerTerm) ||
                           (c.Contact != null && c.Contact.ToLower().Contains(lowerTerm)) ||
                           c.NomSociete.ToLower().Contains(lowerTerm) ||
                           c.CIN.ToLower().Contains(lowerTerm))
                .AsNoTracking()
                .OrderByDescending(c => c.DateCreation)
                .ToListAsync();

            var dtos = _mapper.Map<List<ClientDto>>(clients);
            return Ok(dtos);
        }

        // GET: api/Clients/check-numero?numero=xxx&excludeId=1
        [HttpGet("check-numero")]
        public async Task<ActionResult<bool>> CheckNumeroExists([FromQuery] string numero, [FromQuery] int? excludeId = null)
        {
            var exists = await _context.Clients
                .AnyAsync(c => c.NumeroClient.ToLower() == numero.ToLower() && c.ID != excludeId);
            return Ok(exists);
        }

        // GET: api/Clients/check-cin?cin=xxx&excludeId=1
        [HttpGet("check-cin")]
        public async Task<ActionResult<bool>> CheckCINExists([FromQuery] string cin, [FromQuery] int? excludeId = null)
        {
            var exists = await _context.Clients
                .AnyAsync(c => c.CIN.ToLower() == cin.ToLower() && c.ID != excludeId);
            return Ok(exists);
        }

        // POST: api/Clients
        [HttpPost]
        public async Task<ActionResult<ClientDto>> CreateClient([FromBody] ClientDto dto)
        {
            var entity = _mapper.Map<Client>(dto);
            entity.DateCreation = DateTime.UtcNow;

            _context.Clients.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<ClientDto>(entity);
            return CreatedAtAction(nameof(GetClient), new { id = entity.ID }, resultDto);
        }

        // PUT: api/Clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientDto dto)
        {
            if (id != dto.ID)
                return BadRequest("ID mismatch.");

            var existing = await _context.Clients.FindAsync(id);
            if (existing == null)
                return NotFound();

            _mapper.Map(dto, existing);
            existing.DateModification = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Clients/5/credit-balance
        [HttpGet("{id}/credit-balance")]
        public async Task<ActionResult<object>> GetClientCreditBalance(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound();

            var bilanCredit = await _context.BilansCredit
                .FirstOrDefaultAsync(b => b.ClientID == id);

            if (bilanCredit == null)
            {
                return Ok(new { Balance = 0m, ClientID = id, TotalCredit = 0m, TotalPaye = 0m, CreditFacture = 0m, CreditNonFacture = 0m });
            }

            return Ok(new { 
                bilanCredit.Balance, 
                bilanCredit.ClientID,
                bilanCredit.TotalCredit,
                bilanCredit.TotalPaye,
                bilanCredit.CreditFacture,
                bilanCredit.CreditNonFacture
            });
        }
    }
}
