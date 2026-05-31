using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Models;

namespace FitnessTracker.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AssignmentsController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // GET /api/assignments/unassigned/{trainerId}
    // Returns all clients NOT yet assigned to this trainer.
    [HttpGet("unassigned/{trainerId:int}")]
    public async Task<IActionResult> GetUnassignedClients(int trainerId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var assignedIds = await db.Assignments
            .Where(a => a.TrainerId == trainerId)
            .Select(a => a.ClientId)
            .ToListAsync();

        var unassigned = await db.Clients
            .Where(c => !assignedIds.Contains(c.ClientId))
            .Select(c => new { c.ClientId, c.Username, c.Email })
            .OrderBy(c => c.Username)
            .ToListAsync();

        return Ok(unassigned);
    }

    // POST /api/assignments
    // Body: { "trainerId": 1, "clientId": 2 }
    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] AssignRequest req)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        bool exists = await db.Assignments
            .AnyAsync(a => a.TrainerId == req.TrainerId && a.ClientId == req.ClientId);

        if (!exists)
        {
            db.Assignments.Add(new Assignment { TrainerId = req.TrainerId, ClientId = req.ClientId });
            await db.SaveChangesAsync();
        }

        return Ok(new { message = "Assigned." });
    }

    // DELETE /api/assignments/{trainerId}/{clientId}
    [HttpDelete("{trainerId:int}/{clientId:int}")]
    public async Task<IActionResult> Unassign(int trainerId, int clientId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var assignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.TrainerId == trainerId && a.ClientId == clientId);

        if (assignment == null) return NotFound();

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
        return Ok(new { message = "Unassigned." });
    }
}

public record AssignRequest(int TrainerId, int ClientId);
