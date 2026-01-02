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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, IMapper mapper, ILogger<UsersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            _logger.LogInformation("GetUsers called");
            
            var users = await _context.Users.AsNoTracking().ToListAsync();
            _logger.LogInformation("Found {Count} users", users.Count);
            
            return Ok(_mapper.Map<List<UserDto>>(users));
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<UserDto>(user));
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDto userDto)
        {
            if (id != userDto.UserId)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Preserve the existing password if no new password is provided
            var existingPassword = existingUser.Password;

            // Map all fields from DTO to entity
            _mapper.Map(userDto, existingUser);
            
            // Restore password if it wasn't explicitly provided in the update
            if (string.IsNullOrEmpty(userDto.Password))
            {
                existingUser.Password = existingPassword;
            }
            
            existingUser.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(UserDto userDto)
        {
            _logger.LogInformation("Creating user '{Name}', Email: {Email}, Password provided: {HasPassword}", 
                userDto.Name, userDto.Email, !string.IsNullOrEmpty(userDto.Password));
            
            if (string.IsNullOrEmpty(userDto.Password))
            {
                _logger.LogWarning("User creation attempted without password!");
                return BadRequest("Password is required");
            }
            
            var user = _mapper.Map<User>(userDto);
            
            _logger.LogInformation("Mapped user password is: {HasPassword}", !string.IsNullOrEmpty(user.Password));
            
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User created with ID: {UserId}, Password stored: {HasPassword}", 
                user.UserId, !string.IsNullOrEmpty(user.Password));

            var resultDto = _mapper.Map<UserDto>(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, resultDto);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        // GET: api/Users/Login/{Email}/{Password}
        [HttpGet("Login/{Email}/{Password}")]
        public async Task<ActionResult<UserDto>> Login(string Email, string Password)
        {
            _logger.LogInformation("Login attempt for email: {Email}", Email);
            
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                _logger.LogWarning("Login failed: empty email or password");
                return BadRequest();
            }

            // First, find the user by email to see if they exist
            var userByEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == Email);
            
            if (userByEmail == null)
            {
                // List all users in database for debugging
                var allUsers = await _context.Users.Select(u => u.Email).ToListAsync();
                _logger.LogWarning("Login failed: no user found with email {Email}. Available emails in database: [{Emails}]", 
                    Email, string.Join(", ", allUsers));
                return NotFound();
            }
            
            _logger.LogInformation("User found: {Name}, StoredPassword length: {Length}, ProvidedPassword length: {ProvidedLength}", 
                userByEmail.Name, 
                userByEmail.Password?.Length ?? 0,
                Password.Length);
            
            // Check password match
            if (userByEmail.Password != Password)
            {
                _logger.LogWarning("Login failed: password mismatch for user {Email}. Stored: '{Stored}', Provided: '{Provided}'", 
                    Email, userByEmail.Password, Password);
                return NotFound();
            }

            _logger.LogInformation("Login successful for user: {Name}", userByEmail.Name);
            return Ok(_mapper.Map<UserDto>(userByEmail));
        }
    }
}
