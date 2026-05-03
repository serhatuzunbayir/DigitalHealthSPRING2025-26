using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Models;

namespace FitnessTracker.Services;

public class AuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Client?> RegisterAsync(string username, string email, string password)
    {
        bool exists = await _db.Clients.AnyAsync(c => c.Username == username || c.Email == email);
        if (exists) return null;

        var client = new Client
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return client;
    }

    public async Task<Client?> LoginAsync(string username, string password)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Username == username);
        if (client == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, client.PasswordHash)) return null;
        return client;
    }
}
