using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using FourniProApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(AppDbContext context, IMapper mapper, ILogger<ClientsController> logger)
    : ControllerBase
{
    // GET: api/Clients
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
    {
        var clients = await context.Clients.AsNoTracking().ToListAsync();
        var dtos = mapper.Map<List<ClientDto>>(clients);
        await ResolveUserNamesAsync(dtos);
        return Ok(dtos);
    }

    // GET: api/Clients/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDto>> GetClient(int id)
    {
        var client = await context.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClientId == id);

        if (client is null) return NotFound();

        var dto = mapper.Map<ClientDto>(client);
        await ResolveUserNamesAsync([dto]);
        return Ok(dto);
    }

    // POST: api/Clients
    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient(ClientDto dto)
    {
        var client = mapper.Map<Client>(dto);
        client.DateCreation = DateTime.UtcNow;
        client.DateModification = DateTime.UtcNow;

        context.Clients.Add(client);
        await context.SaveChangesAsync();

        logger.LogInformation("Client créé: {Nom} (Id={Id})", client.Nom, client.ClientId);
        return CreatedAtAction(nameof(GetClient), new { id = client.ClientId }, mapper.Map<ClientDto>(client));
    }

    // PUT: api/Clients/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateClient(int id, ClientDto dto)
    {
        var client = await context.Clients.FindAsync(id);
        if (client is null) return NotFound();

        mapper.Map(dto, client);
        client.ClientId = id; // guard against DTO overwrite
        client.DateModification = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Client mis à jour: Id={Id}", id);
        return NoContent();
    }

    // DELETE: api/Clients/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await context.Clients.FindAsync(id);
        if (client is null) return NotFound();

        context.Clients.Remove(client);
        await context.SaveChangesAsync();

        logger.LogInformation("Client supprimé: Id={Id}", id);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves <see cref="ClientDto.AjouteParNom"/> and <see cref="ClientDto.ModifieParNom"/>
    /// from the Users table for all DTOs in a single batch query.
    /// </summary>
    private async Task ResolveUserNamesAsync(IList<ClientDto> dtos)
    {
        var ids = dtos
            .SelectMany(d => new[] { d.AjoutePar, d.ModifiePar })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0) return;

        var names = await context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, u => u.Name);

        foreach (var dto in dtos)
        {
            if (dto.AjoutePar.HasValue && names.TryGetValue(dto.AjoutePar.Value, out var nom))
                dto.AjouteParNom = nom;
            if (dto.ModifiePar.HasValue && names.TryGetValue(dto.ModifiePar.Value, out var modNom))
                dto.ModifieParNom = modNom;
        }
    }
}
