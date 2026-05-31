using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Models;

namespace FitnessTracker.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public SessionsController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // GET /api/sessions/trainer/{trainerId}
    // Returns all sessions for a trainer with client names (req #6, #7).
    [HttpGet("trainer/{trainerId:int}")]
    public async Task<IActionResult> GetSessionsByTrainer(int trainerId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var sessions = await db.VirtualSessions
            .Where(s => s.TrainerId == trainerId)
            .Include(s => s.Client)
            .OrderByDescending(s => s.SessionTime)
            .Select(s => new
            {
                s.SessionId,
                ClientName = s.Client.Username,
                s.ClientId,
                s.SessionTime,
                s.DurationMinutes,
                s.Status
            })
            .ToListAsync();

        return Ok(sessions);
    }

    // GET /api/sessions/client/{clientId}
    // Returns upcoming and recent sessions for a client.
    [HttpGet("client/{clientId:int}")]
    public async Task<IActionResult> GetSessionsByClient(int clientId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var sessions = await db.VirtualSessions
            .Where(s => s.ClientId == clientId && s.SessionTime >= DateTime.UtcNow.AddDays(-1))
            .Include(s => s.Trainer)
            .OrderBy(s => s.SessionTime)
            .Select(s => new
            {
                s.SessionId,
                TrainerName = s.Trainer.Username,
                s.TrainerId,
                s.SessionTime,
                s.DurationMinutes,
                s.Status
            })
            .ToListAsync();

        return Ok(sessions);
    }

    // POST /api/sessions
    // Client requests a new session (req #6).
    // Body: { "clientId": 1, "trainerId": 2, "sessionTime": "2026-06-01T10:00:00", "durationMinutes": 60 }
    [HttpPost]
    public async Task<IActionResult> RequestSession([FromBody] SessionRequest req)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var session = new VirtualSession
        {
            ClientId = req.ClientId,
            TrainerId = req.TrainerId,
            SessionTime = req.SessionTime,
            DurationMinutes = req.DurationMinutes,
            Status = "scheduled"
        };

        db.VirtualSessions.Add(session);
        await db.SaveChangesAsync();
        return Ok(new { session.SessionId, message = "Session scheduled." });
    }

    // PATCH /api/sessions/{sessionId}/status
    // Trainer updates session status: scheduled → active → completed | canceled (req #7).
    // Body: { "status": "active" }
    [HttpPatch("{sessionId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int sessionId, [FromBody] StatusUpdate req)
    {
        var allowed = new[] { "scheduled", "active", "completed", "canceled" };
        if (!allowed.Contains(req.Status))
            return BadRequest($"Invalid status. Allowed: {string.Join(", ", allowed)}");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var session = await db.VirtualSessions.FindAsync(sessionId);
        if (session == null) return NotFound();

        session.Status = req.Status;
        await db.SaveChangesAsync();
        return Ok(new { session.SessionId, session.Status });
    }
}

public record SessionRequest(int ClientId, int TrainerId, DateTime SessionTime, int DurationMinutes);
public record StatusUpdate(string Status);
