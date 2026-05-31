using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;

namespace FitnessTracker.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ClientsController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // GET /api/clients/assigned/{trainerId}
    // Returns all clients assigned to the given trainer.
    [HttpGet("assigned/{trainerId:int}")]
    public async Task<IActionResult> GetAssignedClients(int trainerId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var clients = await db.Assignments
            .Where(a => a.TrainerId == trainerId)
            .Include(a => a.Client)
            .Select(a => new
            {
                a.Client.ClientId,
                a.Client.Username,
                a.Client.Email
            })
            .OrderBy(c => c.Username)
            .ToListAsync();

        return Ok(clients);
    }

    // GET /api/clients/dashboard/{trainerId}
    // Returns dashboard summary for every assigned client — uses LINQ analytics
    // (req #8: Client Monitoring Dashboard, req #4: Progress Tracking & Analytics).
    [HttpGet("dashboard/{trainerId:int}")]
    public async Task<IActionResult> GetDashboardSummary(int trainerId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var assignedClientIds = await db.Assignments
            .Where(a => a.TrainerId == trainerId)
            .Select(a => a.ClientId)
            .ToListAsync();

        if (!assignedClientIds.Any()) return Ok(Array.Empty<object>());

        var weekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Load all exercise logs and goals for assigned clients in two queries
        var allLogs = await db.ExerciseLogs
            .Where(l => assignedClientIds.Contains(l.ClientId))
            .ToListAsync();

        var allGoals = await db.FitnessGoals
            .Where(g => assignedClientIds.Contains(g.ClientId))
            .ToListAsync();

        var clients = await db.Clients
            .Where(c => assignedClientIds.Contains(c.ClientId))
            .Select(c => new { c.ClientId, c.Username, c.Email })
            .ToListAsync();

        // LINQ grouping and analytics per client (req #4)
        var summaries = clients.Select(client =>
        {
            var logs = allLogs.Where(l => l.ClientId == client.ClientId).ToList();
            var goals = allGoals.Where(g => g.ClientId == client.ClientId).ToList();

            var lastActivity = logs.Any() ? logs.Max(l => l.LogDate) : (DateOnly?)null;
            var weeklyCount = logs.Count(l => l.LogDate >= weekStart && l.LogDate <= today);
            var adherenceRate = goals.Count > 0
                ? Math.Round(goals.Count(g => g.Status == "completed") * 100.0 / goals.Count, 1)
                : 0.0;
            var activeGoals = goals.Count(g => g.Status == "in_progress");

            return new
            {
                client.ClientId,
                client.Username,
                client.Email,
                LastActivity = lastActivity?.ToString("dd/MM/yyyy") ?? "No activity",
                WeeklyExerciseCount = weeklyCount,
                AdherenceRate = adherenceRate,
                ActiveGoals = activeGoals
            };
        }).ToList();

        return Ok(summaries);
    }
}
