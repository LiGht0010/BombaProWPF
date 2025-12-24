using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BombaProMaxApi.Data;
using BombaProMaxApi.Models;
using BombaProMaxApi.DTOs;

namespace BombaProMaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Employes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeDto>>> GetEmployes()
        {
            var employes = await _context.Employes.ToListAsync();
            return Ok(_mapper.Map<List<EmployeDto>>(employes));
        }

        // GET: api/Employes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeDto>> GetEmploye(int id)
        {
            var employe = await _context.Employes.FindAsync(id);

            if (employe == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<EmployeDto>(employe));
        }

        // PUT: api/Employes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmploye(int id, EmployeDto employeDto)
        {
            if (id != employeDto.ID)
            {
                return BadRequest();
            }

            var employe = await _context.Employes.FindAsync(id);
            if (employe == null)
            {
                return NotFound();
            }

            _mapper.Map(employeDto, employe);
            _context.Entry(employe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeExists(id))
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

        // POST: api/Employes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmployeDto>> PostEmploye(EmployeDto employeDto)
        {
            var employe = _mapper.Map<Employe>(employeDto);
            _context.Employes.Add(employe);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<EmployeDto>(employe);
            return CreatedAtAction("GetEmploye", new { id = employe.ID }, resultDto);
        }

        // DELETE: api/Employes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmploye(int id)
        {
            var employe = await _context.Employes.FindAsync(id);
            if (employe == null)
            {
                return NotFound();
            }

            _context.Employes.Remove(employe);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Employes/5/hasrelatedrecords
        [HttpGet("{id}/hasrelatedrecords")]
        public async Task<ActionResult<bool>> HasRelatedRecords(int id)
        {
            var employe = await _context.Employes
                .Include(e => e.Jaugeages)
                .Include(e => e.EmployeBilanCredit)
                .Include(e => e.EmployeCreditTransactions)
                .Include(e => e.EmployeReglementsCredit)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (employe == null)
            {
                return NotFound();
            }

            var hasRelated = employe.Jaugeages.Any() ||
                             employe.EmployeBilanCredit != null ||
                             employe.EmployeCreditTransactions.Any() ||
                             employe.EmployeReglementsCredit.Any();

            return Ok(hasRelated);
        }

        private bool EmployeExists(int id)
        {
            return _context.Employes.Any(e => e.ID == id);
        }
    }
}
