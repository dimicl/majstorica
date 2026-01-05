using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BCrypt.Net;
using backend.Data;
using backend.DTOs;
using backend.Entities;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly IMongoCollection<User>? _users;

    public AuthController(MongoDBService mongoDBService) {
        _users = mongoDBService.Database?.GetCollection<User>("users");
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers() {
        if (_users == null) return NotFound();
        var users = await _users.Find(_ => true).ToListAsync();
        return Ok(users);

        //implementirati login s jwt tokenom kasnije
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto) {
        if (_users == null) return StatusCode(500, "Database not available");
        
        var existingUser = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (existingUser) {
            return BadRequest("Korisnik sa ovim email-om već postoji");
        }
        
        var user = new User {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        await _users.InsertOneAsync(user);    
        return Ok(user);
    }
}