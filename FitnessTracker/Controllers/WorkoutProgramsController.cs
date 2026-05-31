using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Models;

namespace FitnessTracker.Controllers;

[ApiController]
[Route("api/programs")]
public class WorkoutProgramsController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public WorkoutProgramsController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // GET /api/programs/{trainerId}/{clientId}
    // Returns all workout programs a trainer created for a specific client (req #3).
    [HttpGet("{trainerId:int}/{clientId:int}")]
    public async Task<IActionResult> GetPrograms(int trainerId, int clientId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var programs = await db.WorkoutPrograms
            .Where(p => p.TrainerId == trainerId && p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new
            {
                p.ProgramId,
                p.ProgramName,
                p.Description,
                p.CreatedDate
            })
            .ToListAsync();

        return Ok(programs);
    }

    // POST /api/programs
    // Trainer creates a new workout program for a client (req #3).
    // Body: { "trainerId": 1, "clientId": 2, "programName": "...", "description": "..." }
    [HttpPost]
    public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ProgramName))
            return BadRequest("Program name is required.");

        await using var db = await _dbFactory.CreateDbContextAsync();

        var program = new WorkoutProgram
        {
            TrainerId = req.TrainerId,
            ClientId = req.ClientId,
            ProgramName = req.ProgramName,
            Description = req.Description,
            CreatedDate = DateOnly.FromDateTime(DateTime.Today)
        };

        db.WorkoutPrograms.Add(program);
        await db.SaveChangesAsync();
        return Ok(new { program.ProgramId, message = "Program created." });
    }

    // PUT /api/programs/{programId}
    // Trainer updates an existing program (req #3).
    // Body: { "programName": "...", "description": "..." }
    [HttpPut("{programId:int}")]
    public async Task<IActionResult> UpdateProgram(int programId, [FromBody] UpdateProgramRequest req)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var program = await db.WorkoutPrograms.FindAsync(programId);
        if (program == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.ProgramName))
            program.ProgramName = req.ProgramName;

        program.Description = req.Description;
        await db.SaveChangesAsync();
        return Ok(new { program.ProgramId, program.ProgramName, message = "Program updated." });
    }

    // DELETE /api/programs/{programId}
    [HttpDelete("{programId:int}")]
    public async Task<IActionResult> DeleteProgram(int programId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var program = await db.WorkoutPrograms.FindAsync(programId);
        if (program == null) return NotFound();

        db.WorkoutPrograms.Remove(program);
        await db.SaveChangesAsync();
        return Ok(new { message = "Program deleted." });
    }
}

public record CreateProgramRequest(int TrainerId, int ClientId, string ProgramName, string? Description);
public record UpdateProgramRequest(string? ProgramName, string? Description);
