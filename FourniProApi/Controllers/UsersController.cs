using AutoMapper;
using FourniProApi.Data;
using FourniProApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FourniProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext context, IMapper mapper, ILogger<UsersController> logger)
    : ControllerBase
{
    // GET: api/Users/Login/{email}/{password}
    [HttpGet("Login/{email}/{password}")]
    public async Task<ActionResult<UserDto>> Login(string email, string password)
    {
        logger.LogInformation("Login attempt for: {Email}", email);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email et mot de passe requis.");

        var user = await context.Users
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            logger.LogWarning("Login failed — no user with email {Email}", email);
            return NotFound();
        }

        if (user.Password != password)
        {
            logger.LogWarning("Login failed — wrong password for {Email}", email);
            return NotFound();
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login failed — user {Email} is inactive", email);
            return Forbid();
        }

        logger.LogInformation("Login successful for {Name}", user.Name);
        return Ok(mapper.Map<UserDto>(user));
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await context.Users
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .AsNoTracking()
            .ToListAsync();

        return Ok(mapper.Map<List<UserDto>>(users));
    }

    // GET: api/Users/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await context.Users
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user is null) return NotFound();
        return Ok(mapper.Map<UserDto>(user));
    }
}
