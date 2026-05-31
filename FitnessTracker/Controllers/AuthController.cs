using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Models;

namespace FitnessTracker.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AuthController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // POST /api/auth/login
    // Body: { "username": "...", "password": "...", "role": "client"|"trainer" }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        if (req.Role == "trainer")
        {
            // Trainers use SHA-256 (matches the MAUI desktop app)
            var hash = Sha256Hash(req.Password);
            var trainer = await db.Trainers
                .Where(t => t.Username == req.Username && t.PasswordHash == hash)
                .Select(t => new { t.TrainerId, t.Username, t.Email })
                .FirstOrDefaultAsync();

            if (trainer == null) return Unauthorized("Invalid credentials.");
            return Ok(trainer);
        }
        else
        {
            // Clients use BCrypt (matches the Blazor web app)
            var client = await db.Clients
                .FirstOrDefaultAsync(c => c.Username == req.Username);

            if (client == null || !BCrypt.Net.BCrypt.Verify(req.Password, client.PasswordHash))
                return Unauthorized("Invalid credentials.");

            return Ok(new { client.ClientId, client.Username, client.Email });
        }
    }

    // POST /api/auth/register
    // Body: { "username": "...", "password": "...", "email": "...", "role": "client"|"trainer" }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        if (req.Role == "trainer")
        {
            if (await db.Trainers.AnyAsync(t => t.Username == req.Username))
                return Conflict("Username already exists.");

            var hash = Sha256Hash(req.Password);
            db.Trainers.Add(new Trainer { Username = req.Username, PasswordHash = hash, Email = req.Email });
        }
        else
        {
            if (await db.Clients.AnyAsync(c => c.Username == req.Username))
                return Conflict("Username already exists.");

            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            db.Clients.Add(new Client { Username = req.Username, PasswordHash = hash, Email = req.Email });
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Registered successfully." });
    }

    // SHA-256 matching the MAUI desktop app's AuthService
    private static string Sha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}

public record LoginRequest(string Username, string Password, string Role = "client");
public record RegisterRequest(string Username, string Password, string Email, string Role = "client");
